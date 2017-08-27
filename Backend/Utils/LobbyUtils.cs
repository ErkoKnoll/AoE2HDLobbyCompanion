using Backend.Constants;
using Backend.Global;
using Commons.Constants;
using Commons.Models;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commons.Extensions;
using Commons.Models.Commands;
using Microsoft.EntityFrameworkCore;

namespace Backend.Utils {
    public class LobbyUtils {
        public static void ResetLobbyData() {
            Variables.Lobby = new Lobby();
            lock (Variables.LobbyPlayers) {
                Variables.LobbyPlayers = new List<Player>();
                for (var i = 0; i < 8; i++) {
                    Variables.LobbyPlayers.Add(new Player());
                }
            }
            Variables.PlayerGameStatsCache = new Dictionary<ulong, PlayerGameStats>();
            Variables.PlayerProfilesCache = new Dictionary<ulong, PlayerProfile>();
            Variables.PlayerReputationStatsCache = new Dictionary<ulong, PlayerReputationStats>();
            if (Variables.OverlayWindow != null) {
                Variables.OverlayWindow.HideMessage();
            }
        }

        public static void StartLobby(Lobby lobby) {
            try {
                LobbySessionStop();
                PersistLobbyStart(lobby);
                CommandUtils.QueueCommand(new BaseCommand() {
                    Id = (int)Commands.OUT.GAME_STARTED
                });
            } catch (Exception e) {
                LogUtils.Error("Failed to process lobby start for lobby: " + lobby.LobbyId, e);
            }
        }

        private static void PersistLobbyStart(Lobby lobby) {
            try {
                using (IRepository repository = new Repository()) {
                    var persistedLobby = repository.Lobbies.Include(l => l.Players).FirstOrDefault(l => l.LobbyId == lobby.LobbyId);
                    if (persistedLobby != null) {
                        foreach (var player in persistedLobby.Players) {
                            if (player.User != null) {
                                player.Position = 0;
                                repository.SetModified(player);
                            } else {
                                repository.Delete(player);
                            }
                        }
                        foreach (var player in lobby.Players) {
                            try {
                                if (player.SteamId != 0) {
                                    var lobbyPlayer = UserUtils.GetUser(repository, player);
                                    var lobbySlot = repository.LobbySlots.Include(ls => ls.User).GetLobbySlot(player.SteamId, lobby.LobbyId).FirstOrDefault();
                                    if (lobbySlot == null) {
                                        lobbySlot = new Database.Domain.LobbySlot() {
                                            Name = player.Name,
                                            RankDM = player.RankDM,
                                            RankRM = player.RankRM,
                                            User = lobbyPlayer,
                                            Lobby = persistedLobby
                                        };
                                        repository.Add(lobbySlot);
                                        repository.SaveChanges();
                                        AssignLobbySlotId(lobbySlot);
                                    } else {
                                        repository.SetModified(lobbySlot);
                                    }
                                    lobbySlot.Position = player.Position;
                                    lobbyPlayer.Games++;
                                    repository.SetModified(lobbyPlayer);
                                } else {
                                    repository.Add(new Database.Domain.LobbySlot() {
                                        Lobby = persistedLobby,
                                        Name = player.Name,
                                        Position = player.Position,
                                    });
                                }
                            } catch (Exception e) {
                                LogUtils.Error(string.Format("Failed to finalize player - User {0}({1}) - Lobby: {2}", player.Name, player.SteamId, lobby.LobbyId), e);
                            }
                        }
                        persistedLobby.Started = DateTime.UtcNow;
                        persistedLobby.Sealed = false;
                        repository.SetModified(persistedLobby);
                        repository.SaveChanges();
                    }
                }
            } catch (Exception e) {
                LogUtils.Error("Failed to persist lobby start for lobby: " + lobby.LobbyId, e);
            }
        }

        public static void AssignLobbySlotId(Database.Domain.LobbySlot lobbySlot, Player player = null) {
            if (player == null) {
                player = Variables.Lobby.Players.FirstOrDefault(p => p.SteamId == lobbySlot.User.SteamId);
            }
            if (player == null) {
                LogUtils.Warn(string.Format("Could not assign lobby slot id for player: {0}({1})", lobbySlot.User.Name, lobbySlot.User.SteamId));
            } else {
                player.LobbySlotId = lobbySlot.Id;
            }
        }

        public static void CalculateLobbyPlayerFieldColors() {
            try {
                var players = new List<Player>();
                lock (Variables.LobbyPlayers) {
                    players.AddRange(Variables.LobbyPlayers);
                }
                foreach (var player in players) {
                    CalculateUserFieldColors(player, Variables.Lobby.Ranked);
                }
            } catch (Exception e) {
                LogUtils.Error("Error while calculating field colors", e);
            }
        }

        public static void CalculateUserFieldColors(BasePlayer player, int ranked) {
            player.FieldColors = new Dictionary<string, int> {
                [PlayerFields.GAMES] = (int)PlayerFieldColors.NONE,
                [PlayerFields.POSITIVE_REPUTATION] = (int)PlayerFieldColors.NONE,
                [PlayerFields.NEGATIVE_REPUTATION] = (int)PlayerFieldColors.NONE,
                [PlayerFields.TOTAL_GAMES] = (int)PlayerFieldColors.NONE,
                [PlayerFields.WIN_RATIO] = (int)PlayerFieldColors.NONE,
                [PlayerFields.DROP_RATIO] = (int)PlayerFieldColors.NONE,
                [PlayerFields.TOTAL_GAMES_RM] = (int)PlayerFieldColors.NONE,
                [PlayerFields.WIN_RATIO_RM] = (int)PlayerFieldColors.NONE,
                [PlayerFields.DROP_RATIO_RM] = (int)PlayerFieldColors.NONE,
                [PlayerFields.TOTAL_GAMES_DM] = (int)PlayerFieldColors.NONE,
                [PlayerFields.WIN_RATIO_DM] = (int)PlayerFieldColors.NONE,
                [PlayerFields.DROP_RATIO_DM] = (int)PlayerFieldColors.NONE
            };
            if (player.GameStats?.TotalGamesDM < 10) {
                player.FieldColors[PlayerFields.TOTAL_GAMES_DM] = (int)PlayerFieldColors.DANGER;
                if (ranked == 2) {
                    player.FieldColors[PlayerFields.TOTAL_GAMES] = (int)PlayerFieldColors.DANGER;
                }
            } else if (player.GameStats?.TotalGamesDM >= 100) {
                player.FieldColors[PlayerFields.TOTAL_GAMES_DM] = (int)PlayerFieldColors.SAFE;
                if (ranked == 2) {
                    player.FieldColors[PlayerFields.TOTAL_GAMES] = (int)PlayerFieldColors.SAFE;
                }
            }
            if (player.GameStats?.TotalGamesRM < 10) {
                player.FieldColors[PlayerFields.TOTAL_GAMES_RM] = (int)PlayerFieldColors.DANGER;
                if (ranked != 2) {
                    player.FieldColors[PlayerFields.TOTAL_GAMES] = (int)PlayerFieldColors.DANGER;
                }
            } else if (player.GameStats?.TotalGamesRM >= 100) {
                player.FieldColors[PlayerFields.TOTAL_GAMES_RM] = (int)PlayerFieldColors.SAFE;
                if (ranked != 2) {
                    player.FieldColors[PlayerFields.TOTAL_GAMES] = (int)PlayerFieldColors.SAFE;
                }
            }
            if (player.GameStats?.WinRatioDM < 40 && player.GameStats?.TotalGamesDM > 0) {
                player.FieldColors[PlayerFields.WIN_RATIO_DM] = (int)PlayerFieldColors.DANGER;
                if (ranked == 2) {
                    player.FieldColors[PlayerFields.WIN_RATIO] = (int)PlayerFieldColors.DANGER;
                }
            } else if (player.GameStats?.WinRatioDM >= 60 && player.GameStats?.TotalGamesDM > 0) {
                player.FieldColors[PlayerFields.WIN_RATIO_DM] = (int)PlayerFieldColors.SAFE;
                if (ranked == 2) {
                    player.FieldColors[PlayerFields.WIN_RATIO] = (int)PlayerFieldColors.SAFE;
                }
            }
            if (player.GameStats?.WinRatioRM < 40 && player.GameStats?.TotalGamesRM > 0) {
                player.FieldColors[PlayerFields.WIN_RATIO_RM] = (int)PlayerFieldColors.DANGER;
                if (ranked != 2) {
                    player.FieldColors[PlayerFields.WIN_RATIO] = (int)PlayerFieldColors.DANGER;
                }
            } else if (player.GameStats?.DropRatioRM >= 60 && player.GameStats?.TotalGamesRM > 0) {
                player.FieldColors[PlayerFields.WIN_RATIO_RM] = (int)PlayerFieldColors.SAFE;
                if (ranked != 2) {
                    player.FieldColors[PlayerFields.WIN_RATIO] = (int)PlayerFieldColors.SAFE;
                }
            }
            if (player.GameStats?.DropRatioDM == 0 && player.GameStats?.TotalGamesDM > 0) {
                player.FieldColors[PlayerFields.DROP_RATIO_DM] = (int)PlayerFieldColors.SAFE;
                if (ranked == 2) {
                    player.FieldColors[PlayerFields.DROP_RATIO] = (int)PlayerFieldColors.SAFE;
                }
            } else if (player.GameStats?.DropRatioDM >= 10 && player.GameStats?.TotalGamesDM > 0) {
                player.FieldColors[PlayerFields.DROP_RATIO_DM] = (int)PlayerFieldColors.DANGER;
                if (ranked == 2) {
                    player.FieldColors[PlayerFields.DROP_RATIO] = (int)PlayerFieldColors.DANGER;
                }
            }
            if (player.GameStats?.DropRatioRM == 0 && player.GameStats?.TotalGamesRM > 0) {
                player.FieldColors[PlayerFields.DROP_RATIO_RM] = (int)PlayerFieldColors.SAFE;
                if (ranked != 2) {
                    player.FieldColors[PlayerFields.DROP_RATIO] = (int)PlayerFieldColors.SAFE;
                }
            } else if (player.GameStats?.DropRatioRM >= 10 && player.GameStats?.TotalGamesRM > 0) {
                player.FieldColors[PlayerFields.DROP_RATIO_RM] = (int)PlayerFieldColors.DANGER;
                if (ranked != 2) {
                    player.FieldColors[PlayerFields.DROP_RATIO] = (int)PlayerFieldColors.DANGER;
                }
            }
            if (player.ReputationStats?.Games > 0 && player.ReputationStats?.NegativeReputation == 0) {
                player.FieldColors[PlayerFields.GAMES] = (int)PlayerFieldColors.SAFE;
            }
            if (player.ReputationStats?.PositiveReputation > 0) {
                player.FieldColors[PlayerFields.POSITIVE_REPUTATION] = (int)PlayerFieldColors.SAFE;
            }
            if (player.ReputationStats?.NegativeReputation > 0) {
                player.FieldColors[PlayerFields.NEGATIVE_REPUTATION] = (int)PlayerFieldColors.DANGER;
            }
        }

        public static void LobbySessionStop() {
            Variables.LobbySession = false;
            if (Variables.OverlayWindow != null) {
                Variables.OverlayWindow.UpdateConfiguration(Variables.Config, Variables.LobbySession);
            }
        }
    }
}
