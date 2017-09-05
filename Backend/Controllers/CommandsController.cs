using Backend.Constants;
using Backend.Global;
using Backend.Jobs;
using Backend.Utils;
using Commons.Models;
using Commons.Models.Commands;
using Database;
using Database.Domain;
using Microsoft.AspNetCore.Mvc;
using Overlay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Newtonsoft.Json;
using Backend.Models;

namespace Backend.Controllers {
    [Route("api/[controller]")]
    public class CommandsController : IDisposable {
        private IRepository _repository;

        public CommandsController(IRepository repository) {
            _repository = repository;
        }

        public IEnumerable<BaseCommand> Get() {
            lock (Variables.OutgoingCommandsQueue) {
                var commands = new List<BaseCommand>();
                commands.AddRange(Variables.OutgoingCommandsQueue);
                Variables.OutgoingCommandsQueue.Clear();
                return commands;
            }
        }

        [HttpPut("{id}")]
        public object Put(int id, [FromBody]string payload = null) {
            switch (id) {
                case (int)Commands.IN.LOBBY_SESSION_START:
                    ProcessLobbySessionStart();
                    return new { };
                case (int)Commands.IN.LOBBY_SESSION_STOP:
                    ProcessLobbySessionStop();
                    return new { };
                case (int)Commands.IN.SET_UP:
                    return SetUp();
                case (int)Commands.IN.START_LOBBY_MANUALLY:
                    LobbyUtils.StartLobby(Variables.Lobby);
                    return new { };
                case (int)Commands.IN.SEAL_LOBBY:
                    SealLobby(payload);
                    return new { };
                case (int)Commands.IN.GET_UNSEALED_LOBBY:
                    return GetUnsealedLobby();
                case (int)Commands.IN.CALCULATE_BALANCED_TEAMS_BASED_ON_RANK:
                    return CalculateBalancedTeamsBasedOnRank();
                case (int)Commands.IN.CALCULATE_BALANCED_TEAMS_BASED_ON_TOTAL_GAMES:
                    return CalculateBalancedTeamsBasedOnTotalGames();
                case (int)Commands.IN.CALCULATE_BALANCED_TEAMS_BASED_ON_WIN_RATIO:
                    return CalculateBalancedTeamsBasedOnWinRatio();
                case (int)Commands.IN.COPY_PLAYER_STATS:
                    return CopyPlayerStats();
                default:
                    throw new Exception("Command not recognized: " + id);
            }
        }

        private string CalculateBalancedTeamsBasedOnRank() {
            try {
                var hasEvenNumberOfPlayers = CheckIfLobbyHasEvenNumberOfPlayers();
                if (hasEvenNumberOfPlayers != null) {
                    return hasEvenNumberOfPlayers;
                }
                var balanceCalculations = Variables.Lobby.Players.Where(p => p.SteamId > 0).Select(p => new BalanceCalculation {
                    Player = p,
                    Value = p.Rank
                });
                var bestMatch = FindBestMatch(balanceCalculations).ToList();
                var teams = "";
                var players = bestMatch.Select(bc => bc.Player).ToList();
                foreach (var balanceCalculation in balanceCalculations) {
                    teams += (players.IndexOf(balanceCalculation.Player) < balanceCalculations.Count() / 2) ? "1" : "2";
                }
                var team1 = bestMatch.Take(balanceCalculations.Count() / 2).OrderByDescending(p => p.Value);
                var team2 = bestMatch.Skip(balanceCalculations.Count() / 2).OrderByDescending(p => p.Value);
                var team1RankSum = team1.Sum(bc => bc.Value);
                var team2RankSum = team2.Sum(bc => bc.Value);
                var output = "Balanced Teams (Rank): " + teams + " | Team 1: " + (team1RankSum / (balanceCalculations.Count() / 2)) + " - Team 2: " + (team2RankSum / (balanceCalculations.Count() / 2)) + " | Calculated using Lobby Companion [aoe2lc.net]";
                LogUtils.Info(output);
                CopyToClipboard(output);
                return null;
            } catch (Exception e) {
                LogUtils.Error("Error while calculating balanced teams", e);
                return "Error while calculating balanced teams, check the logs.";
            }
        }

        private string CalculateBalancedTeamsBasedOnTotalGames() {
            try {
                var hasEvenNumberOfPlayers = CheckIfLobbyHasEvenNumberOfPlayers();
                if (hasEvenNumberOfPlayers != null) {
                    return hasEvenNumberOfPlayers;
                }
                var memberHasPrivateProfile = CheckIfAnyLobbyMemberHasPrivateProfile();
                if (memberHasPrivateProfile != null) {
                    return memberHasPrivateProfile;
                }
                var balanceCalculations = Variables.Lobby.Players.Where(p => p.SteamId > 0).Select(p => new BalanceCalculation {
                    Player = p,
                    Value = Variables.Lobby.Ranked != 2 ? p.GameStats.TotalGamesRM : p.GameStats.TotalGamesDM
                });
                var bestMatch = FindBestMatch(balanceCalculations).ToList();
                var teams = "";
                var players = bestMatch.Select(bc => bc.Player).ToList();
                foreach (var balanceCalculation in balanceCalculations) {
                    teams += (players.IndexOf(balanceCalculation.Player) < balanceCalculations.Count() / 2) ? "1" : "2";
                }
                var team1 = bestMatch.Take(balanceCalculations.Count() / 2).OrderByDescending(p => p.Value);
                var team2 = bestMatch.Skip(balanceCalculations.Count() / 2).OrderByDescending(p => p.Value);
                var team1RankSum = team1.Sum(bc => bc.Value);
                var team2RankSum = team2.Sum(bc => bc.Value);
                var output = "Balanced Teams (Total Games): " + teams + " | Team 1: " + (team1RankSum / (balanceCalculations.Count() / 2)) + " - Team 2: " + (team2RankSum / (balanceCalculations.Count() / 2)) + " | Calculated using Lobby Companion [aoe2lc.net]";
                LogUtils.Info(output);
                CopyToClipboard(output);
                return null;
            } catch (Exception e) {
                LogUtils.Error("Error while calculating balanced teams", e);
                return "Error while calculating balanced teams, check the logs.";
            }
        }

        private string CalculateBalancedTeamsBasedOnWinRatio() {
            try {
                var hasEvenNumberOfPlayers = CheckIfLobbyHasEvenNumberOfPlayers();
                if (hasEvenNumberOfPlayers != null) {
                    return hasEvenNumberOfPlayers;
                }
                var memberHasPrivateProfile = CheckIfAnyLobbyMemberHasPrivateProfile();
                if (memberHasPrivateProfile != null) {
                    return memberHasPrivateProfile;
                }
                var balanceCalculations = Variables.Lobby.Players.Where(p => p.SteamId > 0).Select(p => new BalanceCalculation {
                    Player = p,
                    Value = Variables.Lobby.Ranked != 2 ? p.GameStats.WinRatioRM : p.GameStats.WinRatioDM
                });
                var bestMatch = FindBestMatch(balanceCalculations).ToList();
                var teams = "";
                var players = bestMatch.Select(bc => bc.Player).ToList();
                foreach (var balanceCalculation in balanceCalculations) {
                    teams += (players.IndexOf(balanceCalculation.Player) < balanceCalculations.Count() / 2) ? "1" : "2";
                }
                var team1 = bestMatch.Take(balanceCalculations.Count() / 2).OrderByDescending(p => p.Value);
                var team2 = bestMatch.Skip(balanceCalculations.Count() / 2).OrderByDescending(p => p.Value);
                var team1RankSum = team1.Sum(bc => bc.Value);
                var team2RankSum = team2.Sum(bc => bc.Value);
                var output = "Balanced Teams (Win Ratio): " + teams + " | Team 1: " + (team1RankSum / (balanceCalculations.Count() / 2)) + "% - Team 2: " + (team2RankSum / (balanceCalculations.Count() / 2)) + "% | Calculated using Lobby Companion [aoe2lc.net]";
                LogUtils.Info(output);
                CopyToClipboard(output);
                return null;
            } catch (Exception e) {
                LogUtils.Error("Error while calculating balanced teams", e);
                return "Error while calculating balanced teams, check the logs.";
            }
        }

        private string CopyPlayerStats() {
            try {
                var players = Variables.Lobby.Players.Where(p => p.Profile != null && p.GameStats != null && !(p.Profile.ProfilePrivate && !p.Profile.ProfileDataFetched.HasValue));
                var output = "[aoe2lc.net] Stats (Total Games/Win Ratio/Drop Ratio): " + string.Join(", ", players.Select(p => p.Name + "(" + (Variables.Lobby.Ranked != 2 ? p.GameStats.TotalGamesRM + "/" + p.GameStats.WinRatioRM + "%/" + p.GameStats.DropRatioRM + "%" : p.GameStats.TotalGamesDM + "/" + p.GameStats.WinRatioDM + "%/" + p.GameStats.DropRatioDM + "%") + ")"));
                LogUtils.Info(output);
                CopyToClipboard(output);
                return null;
            } catch (Exception e) {
                LogUtils.Error("Error while copying player stats", e);
                return "Error while copying player stats, check the logs.";
            }
        }

        private void CopyToClipboard(string text) {
            Thread thread = new Thread(() => Clipboard.SetText(text));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private string CheckIfLobbyHasEvenNumberOfPlayers() {
            if (Variables.Lobby.Players.Count(p => p.Rank > 0 && p.SteamId > 0) < 2) {
                return "Lobby must have at least 2 players for balance calculation.";
            } else {
                if (Variables.Lobby.Players.Count(p => p.Rank > 0 && p.SteamId > 0) % 2 == 0) {
                    return null;
                } else {
                    return "Lobby must have even number of players for balance calculation.";
                }
            }
        }

        private string CheckIfAnyLobbyMemberHasPrivateProfile() {
            if (Variables.Lobby.Players.Any(p => p.Rank > 0 && (p.Profile == null || p.GameStats == null || (p.Profile.ProfilePrivate && !p.Profile.ProfileDataFetched.HasValue)))) {
                return "Either some player stats are not fetched yet or some player's profile is private.";
            } else {
                return null;
            }
        }

        private IEnumerable<BalanceCalculation> FindBestMatch(IEnumerable<BalanceCalculation> players) {
            IEnumerable<BalanceCalculation> bestMatch = null;
            var bestLowestDifference = 9999;
            var totalPlayers = players.Count();
            var playersDictionary = players.ToDictionary(p => p.Player.SteamId);
            var permutations = GetPermutations(players.Select(p => p.Player.SteamId), totalPlayers).ToList();
            foreach (var permutation in permutations) {
                var team1Score = 0;
                var team2Score = 0;
                var index = 0;
                var permutationList = new List<BalanceCalculation>();
                foreach (var permutationValue in permutation) {
                    var player = playersDictionary[permutationValue];
                    permutationList.Add(player);
                    if (index < totalPlayers / 2) {
                        team1Score += player.Value;
                    } else {
                        team2Score += player.Value;
                    }
                    index++;
                }
                var difference = Math.Abs(team1Score - team2Score);
                if (difference < bestLowestDifference) {
                    bestLowestDifference = difference;
                    bestMatch = permutationList;
                }
            }
            return bestMatch;
        }

        private string SetUp() {
            try {
                Task.Factory.StartNew(() => CheckForUpdate());
                var path = Path.Combine(Environment.GetEnvironmentVariable("AppData"), "Aoe2HDLobbyCompanion");
                if (!Directory.Exists(path)) {
                    try {
                        Directory.CreateDirectory(path);
                    } catch (Exception e) {
                        return "Failed to create app's data directory in " + path + ". Please ensure you are running the app with enough permissions. Exact error: " + e.Message;
                    }
                }
                try {
                    var directory = SteamUtils.GetSteamDirectory();
                    if (directory == null) {
                        return "Could not find steam directory in registry path: " + SteamUtils.RegistryPathToSteam;
                    }
                } catch (Exception e) {
                    return "Error while finding out steam directory. Exact error: " + e.Message;
                }
                try {
                    _repository.UpdateDatabase();
                    if (!_repository.Reputations.Any()) {
                        _repository.Add(new Database.Domain.Reputation {
                            Name = "Quit",
                            Type = ReputationType.NEGATIVE,
                            OrderSequence = 0
                        });
                        _repository.Add(new Database.Domain.Reputation {
                            Name = "Dropped",
                            Type = ReputationType.NEGATIVE,
                            OrderSequence = 1
                        });
                        _repository.Add(new Database.Domain.Reputation {
                            Name = "Lagged",
                            Type = ReputationType.NEGATIVE,
                            OrderSequence = 2
                        });
                        _repository.Add(new Database.Domain.Reputation {
                            Name = "Desynced",
                            Type = ReputationType.NEGATIVE,
                            OrderSequence = 3
                        });
                        _repository.Add(new Database.Domain.Reputation {
                            Name = "Other",
                            Type = ReputationType.NEGATIVE,
                            CommentRequired = true,
                            OrderSequence = 4
                        });
                        _repository.Add(new Database.Domain.Reputation {
                            Name = "GG",
                            Type = ReputationType.POSITIVE,
                            OrderSequence = 0
                        });
                        _repository.Add(new Database.Domain.Reputation {
                            Name = "Other",
                            Type = ReputationType.POSITIVE,
                            CommentRequired = true,
                            OrderSequence = 1
                        });
                        _repository.SaveChanges();
                    }
                } catch (Exception e) {
                    return "Failed to set up database. Exact error: " + e.Message;
                }
                return null;
            } catch (Exception e) {
                LogUtils.Error("Error while performing setup", e);
                return "Error while performing setup, check the logs.";
            }
        }

        private async Task CheckForUpdate() {
            try {
                using (var client = new HttpClient() { BaseAddress = new Uri("https://raw.githubusercontent.com") }) {
                    var response = await client.GetAsync("/ThorConzales/AoE2HDLobbyCompanion/master/version.json");
                    if (response.IsSuccessStatusCode) {
                        var content = await response.Content.ReadAsStringAsync();
                        var version = JsonConvert.DeserializeObject<Commons.Models.Version>(content);
                        if (Variables.NumericVersion < version.NumericVersion) {
                            CommandUtils.QueueCommand(new UpdateAvailableCommand() {
                                Id = (int)Commands.OUT.UPDATE_AVAILABLE,
                                Version = version
                            });
                        }
                    } else {
                        LogUtils.Error("Failed to check for update. Response code: " + response.StatusCode);
                    }
                }
            } catch (Exception e) {
                LogUtils.Error("Failed to check for update", e);
            }
        }

        private void ProcessLobbySessionStart() {
            LobbyUtils.ResetLobbyData();
            Variables.LobbySession = true;
            if (Variables.OverlayWindow == null) {
                Variables.OverlayThread = new Thread(() => {
                    Variables.OverlayApp = new Application();
                    Variables.OverlayWindow = new MainWindow();
                    Task.Delay(500).ContinueWith(task => Variables.OverlayWindow.UpdateConfiguration(Variables.Config, Variables.LobbySession));
                    Variables.OverlayApp.Run(Variables.OverlayWindow);
                });
                Variables.OverlayThread.SetApartmentState(ApartmentState.STA);
                Variables.OverlayThread.Start();
            } else {
                Variables.OverlayWindow.UpdateConfiguration(Variables.Config, Variables.LobbySession);
            }
            if (Variables.ReplayMode) {
                Task.Factory.StartNew(() => new NetHookDumpReaderJob());
            } else {
                SteamUtils.CheckNethookPaths();
                Task.Delay(5000).ContinueWith(t => SteamUtils.CheckNethookPaths()); //In case NetHook didn't start up fast enough
            }
        }

        private void SealLobby(string lobbyId) {
            var longLobbyId = ulong.Parse(lobbyId);
            var lobby = _repository.Lobbies.Where(l => l.LobbyId == longLobbyId).FirstOrDefault();
            if (lobby != null) {
                lobby.Sealed = true;
                _repository.SetModified(lobby);
                _repository.SaveChanges();
            }
        }

        private void ProcessLobbySessionStop() {
            LobbyUtils.LobbySessionStop();
        }

        private Commons.Models.Lobby GetUnsealedLobby() {
            var runningLobby = _repository.Lobbies.Include(l => l.Players).ThenInclude(ls => ls.User).Where(l => !l.Sealed && l.Started.HasValue).OrderByDescending(l => l.Started).FirstOrDefault();
            if (runningLobby != null) {
                return new Commons.Models.Lobby() {
                    LobbyId = runningLobby.LobbyId,
                    SLobbyId = runningLobby.LobbyId.ToString(),
                    Name = runningLobby.Name
                };
            } else {
                return null;
            }
        }

        private IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length) {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1).SelectMany(t => list.Where(e => !t.Contains(e)), (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        void IDisposable.Dispose() {
            _repository.Dispose();
        }
    }
}
