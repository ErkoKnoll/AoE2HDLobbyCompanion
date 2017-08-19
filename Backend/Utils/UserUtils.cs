using Aoe2LobbyListFetcherService.Models;
using Backend.Global;
using Commons.Models;
using Database;
using Database.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Commons.Extensions;

namespace Backend.Utils {
    public class UserUtils {
        public static Database.Domain.User GetUser(IRepository repository, Player player) {
            var user = repository.Users.FirstOrDefault(u => u.SteamId == player.SteamId);
            if (user == null) {
                user = new Database.Domain.User() {
                    Name = player.Name,
                    SteamId = player.SteamId,
                    RankDM = player.RankDM,
                    RankRM = player.RankRM
                };
                repository.Add(user);
                repository.SaveChanges();
            } else if (user.Name != player.Name || user.RankRM != player.RankRM || user.RankDM != player.RankDM) {
                user.Name = player.Name;
                repository.SetModified(user);
                repository.SaveChanges();
            }
            return user;
        }

        public static void FetchUserGameStats(Player player) {
            try {
                if (player.SteamId == 0) {
                    return;
                }
                if (string.IsNullOrEmpty(Variables.Config?.SteamApiKey)) {
                    return;
                }
                if (Variables.PlayerGameStatsCache == null) {
                    Variables.PlayerGameStatsCache = new Dictionary<ulong, PlayerGameStats>();
                }
                if (Variables.PlayerGameStatsCache.TryGetValue(player.SteamId, out PlayerGameStats playerGameStats)) {
                    player.GameStats = playerGameStats;
                } else {
                    ThreadPool.QueueUserWorkItem(async state => {
                        var p = state as Player;
                        try {
                            using (var client = new HttpClient() { BaseAddress = new Uri("http://api.steampowered.com/") }) {
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                var response = await client.GetAsync(string.Format("/ISteamUserStats/GetUserStatsForGame/v0002/?appid=221380&key={0}&steamid={1}", Variables.Config.SteamApiKey, p.SteamId));
                                if (response.IsSuccessStatusCode) {
                                    var content = await response.Content.ReadAsStringAsync();
                                    var statsResponse = JsonConvert.DeserializeObject<GetPlayerStatsResponse>(content);
                                    int startedRM = 0, endedRM = 0, winsRM = 0, startedDM = 0, endedDM = 0, winsDM = 0;
                                    try {
                                        startedRM = int.Parse(statsResponse.PlayerStats.Stats.Where(s => s.Name == "STAT_ELO_RM_BEGIN").Select(s => s.Value).SingleOrDefault());
                                    } catch { }
                                    try {
                                        startedDM += int.Parse(statsResponse.PlayerStats.Stats.Where(s => s.Name == "STAT_ELO_DM_BEGIN").Select(s => s.Value).SingleOrDefault());
                                    } catch { }
                                    try {
                                        endedRM = int.Parse(statsResponse.PlayerStats.Stats.Where(s => s.Name == "STAT_ELO_RM_END").Select(s => s.Value).SingleOrDefault());
                                    } catch { }
                                    try {
                                        endedDM += int.Parse(statsResponse.PlayerStats.Stats.Where(s => s.Name == "STAT_ELO_DM_END").Select(s => s.Value).SingleOrDefault());
                                    } catch { }
                                    try {
                                        winsRM = int.Parse(statsResponse.PlayerStats.Stats.Where(s => s.Name == "STAT_ELO_RM_WINS").Select(s => s.Value).SingleOrDefault());
                                    } catch { }
                                    try {
                                        winsDM += int.Parse(statsResponse.PlayerStats.Stats.Where(s => s.Name == "STAT_ELO_DM_WINS").Select(s => s.Value).SingleOrDefault());
                                    } catch { }
                                    if (p.GameStats == null) {
                                        CalculateGameStats(p, startedRM, startedDM, winsRM, winsDM, endedRM, endedDM);
                                    }
                                    Variables.PlayerGameStatsCache[p.SteamId] = p.GameStats;
                                    try {
                                        using (IRepository repository = new Repository()) {
                                            var user = repository.Users.FirstOrDefault(u => u.SteamId == p.SteamId);
                                            if (user == null) {
                                                throw new Exception("User not found in database");
                                            }
                                            user.GamesStartedRM = startedRM;
                                            user.GamesStartedDM = startedDM;
                                            user.GamesEndedRM = endedRM;
                                            user.GamesEndedDM = endedDM;
                                            user.GamesWonRM = winsRM;
                                            user.GamesWonDM = winsDM;
                                            user.ProfileDataFetched = DateTime.UtcNow;
                                            repository.SetModified(user);
                                            repository.SaveChanges();
                                            var lobbySlot = repository.LobbySlots.GetLobbySlot(p.SteamId, Variables.Lobby.LobbyId).FirstOrDefault();
                                            if (lobbySlot == null) {
                                                throw new Exception("Lobby slot not found in database");
                                            }
                                            lobbySlot.GamesStartedRM = startedRM;
                                            lobbySlot.GamesStartedDM = startedDM;
                                            lobbySlot.GamesEndedRM = endedRM;
                                            lobbySlot.GamesEndedDM = endedDM;
                                            lobbySlot.GamesWonRM = winsRM;
                                            lobbySlot.GamesWonDM = winsDM;
                                            repository.SetModified(lobbySlot);
                                            repository.SaveChanges();
                                        }
                                    } catch (Exception e) {
                                        LogUtils.Error(string.Format("Failed to persist game stats retrieved from Steam for player: {0}({1})", p?.Name, p?.SteamId), e);
                                    }
                                }
                            }
                        } catch (Exception e) {
                            LogUtils.Error(string.Format("Failed to process games stats for player: {0}({1})", p?.Name, p?.SteamId), e);
                        }
                    }, player);
                }
            } catch (Exception e) {
                LogUtils.Error(string.Format("Failed to retrieve games stats for player: {0}({1})", player?.Name, player?.SteamId), e);
            }
        }

        public static void CalculateGameStats(Player player, int startedRM, int startedDM, int winsRM, int winsDM, int endedRM, int endedDM) {
            player.GameStats = GetGameStats(startedRM, startedDM, winsRM, winsDM, endedRM, endedDM);
        }

        public static PlayerGameStats GetGameStats(int startedRM, int startedDM, int winsRM, int winsDM, int endedRM, int endedDM) {
            return new PlayerGameStats {
                TotalGamesRM = startedRM,
                TotalGamesDM = startedDM,
                WinRatioRM = startedRM > 0 ? winsRM * 100 / startedRM : 0,
                WinRatioDM = startedDM > 0 ? winsDM * 100 / startedDM : 0,
                DropRatioRM = startedRM > 0 ? (startedRM - endedRM) * 100 / startedRM : 0,
                DropRatioDM = startedDM > 0 ? (startedDM - endedDM) * 100 / startedDM : 0
            };
        }

        public static void FetchUserProfile(Player player) {
            try {
                if (player.SteamId == 0) {
                    return;
                }
                if (string.IsNullOrEmpty(Variables.Config?.SteamApiKey)) {
                    return;
                }
                if (Variables.PlayerProfilesCache == null) {
                    Variables.PlayerProfilesCache = new Dictionary<ulong, PlayerProfile>();
                }
                if (Variables.PlayerProfilesCache.TryGetValue(player.SteamId, out PlayerProfile playerProfile)) {
                    player.Profile = playerProfile;
                } else {
                    ThreadPool.QueueUserWorkItem(async state => {
                        var p = state as Player;
                        try {
                            using (var client = new HttpClient() { BaseAddress = new Uri("http://api.steampowered.com/") }) {
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                var response = await client.GetAsync(string.Format("/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}", Variables.Config.SteamApiKey, p.SteamId));
                                Variables.PlayerProfilesCache[p.SteamId] = p.Profile;
                                if (response.IsSuccessStatusCode) {
                                    var content = await response.Content.ReadAsStringAsync();
                                    var profileResponse = JsonConvert.DeserializeObject<GetUsersResponse>(content);
                                    string location = "";
                                    bool profilePrivate = false;
                                    try {
                                        location = profileResponse.Response.Players.FirstOrDefault().LocCountryCode;
                                    } catch { }
                                    try {
                                        profilePrivate = profileResponse.Response.Players.FirstOrDefault().CommunityVisibilityState != 3;
                                    } catch { }
                                    SavePlayerProfile(player, location, profilePrivate);
                                    Variables.PlayerProfilesCache[p.SteamId] = p.Profile;
                                    if (profilePrivate) {
                                        Variables.PlayerGameStatsCache[p.SteamId] = p.GameStats;
                                    }
                                } else {
                                    LogUtils.Error("Failed to fetch player profile. Invalid Steam API Key or Steam service error. Response code: " + response.StatusCode);
                                }
                            }
                        } catch (Exception e) {
                            LogUtils.Error(string.Format("Failed to process profile for player: {0}({1})", p?.Name, p?.SteamId), e);
                            SavePlayerProfile(player, null, true);
                        }
                    }, player);
                }
            } catch (Exception e) {
                LogUtils.Error(string.Format("Failed to retrieve a profile for player: {0}({1})", player?.Name, player?.SteamId), e);
            }
        }

        public static void FetchUserReputationStats(Player player, bool byPassCache = false) {
            try {
                if (player.SteamId == 0) {
                    return;
                }
                if (Variables.PlayerReputationStatsCache == null) {
                    Variables.PlayerReputationStatsCache = new Dictionary<ulong, PlayerReputationStats>();
                }
                if (Variables.PlayerReputationStatsCache.TryGetValue(player.SteamId, out PlayerReputationStats playerReputationStats) && byPassCache == false) {
                    player.ReputationStats = playerReputationStats;
                } else {
                    ThreadPool.QueueUserWorkItem(state => {
                        var p = state as Player;
                        try {
                            using (IRepository repository = new Repository()) {
                                var user = repository.Users.FirstOrDefault(u => u.SteamId == p.SteamId);
                                if (user == null) {
                                    throw new Exception("User not found in database");
                                }
                                player.ReputationStats = new PlayerReputationStats() {
                                    Games = 0,
                                    PositiveReputation = user.PositiveReputation,
                                    NegativeReputation = user.NegativeReputation
                                };
                                Variables.PlayerReputationStatsCache[player.SteamId] = player.ReputationStats;
                            }
                        } catch (Exception e) {
                            LogUtils.Error(string.Format("Failed to read reputation stats for player: {0}({1})", p?.Name, p?.SteamId), e);
                        }

                    }, player);
                }
            } catch (Exception e) {
                LogUtils.Error(string.Format("Failed to retrieve reputation stats for player: {0}({1})", player?.Name, player?.SteamId), e);
            }
        }

        private static void SavePlayerProfile(Player player, string location, bool profilePrivate) {
            player.Profile = new PlayerProfile() {
                Location = location,
                ProfilePrivate = profilePrivate
            };
            if (profilePrivate) {
                player.GameStats = new PlayerGameStats();
            }
            try {
                using (IRepository repository = new Repository()) {
                    var user = repository.Users.FirstOrDefault(u => u.SteamId == player.SteamId);
                    if (user == null) {
                        throw new Exception("User not found in database");
                    }
                    if (profilePrivate && user.ProfileDataFetched.HasValue) {
                        player.Profile.ProfileDataFetched = user.ProfileDataFetched;
                        CalculateGameStats(player, user.GamesStartedRM, user.GamesStartedDM, user.GamesWonRM, user.GamesWonDM, user.GamesEndedRM, user.GamesEndedDM);
                        Variables.PlayerGameStatsCache[player.SteamId] = player.GameStats;
                    }
                    user.Location = location;
                    user.ProfilePrivate = profilePrivate;
                    repository.SetModified(user);
                    repository.SaveChanges();
                    var lobbySlot = repository.LobbySlots.GetLobbySlot(player.SteamId, Variables.Lobby.LobbyId).FirstOrDefault();
                    if (lobbySlot == null) {
                        throw new Exception("Lobby slot not found in database");
                    }
                    lobbySlot.Location = location;
                    repository.SetModified(lobbySlot);
                    repository.SaveChanges();
                }
            } catch (Exception e) {
                LogUtils.Error(string.Format("Error while persisting profile retrieved from Steam for player: {0}({1})", player?.Name, player?.SteamId), e);
            }
        }
    }
}
