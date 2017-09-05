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
        private int _lastMessageId = 0;

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
                    return BalancingUtils.CalculateBalancedTeamsBasedOnRank();
                case (int)Commands.IN.CALCULATE_BALANCED_TEAMS_BASED_ON_TOTAL_GAMES:
                    return BalancingUtils.CalculateBalancedTeamsBasedOnTotalGames();
                case (int)Commands.IN.CALCULATE_BALANCED_TEAMS_BASED_ON_WIN_RATIO:
                    return BalancingUtils.CalculateBalancedTeamsBasedOnWinRatio();
                case (int)Commands.IN.COPY_PLAYER_STATS:
                    return BalancingUtils.CopyPlayerStats();
                default:
                    throw new Exception("Command not recognized: " + id);
            }
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
                    Variables.OverlayWindow.RegisterHotKeyHooks(CopyPlayerStats, CalculateBalancedTeamsRank, CalculateBalancedTeamsTotalGames, CalculateBalancedTeamsWinRatio);
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


        private void CopyPlayerStats() {
            if (Variables.LobbySession) {
                var message = BalancingUtils.CopyPlayerStats();
                if (message == null) {
                    Variables.OverlayWindow.ShowMessage("Player stats were copied to your clipboard, you can paste them to lobby chat.", true);
                    SetMessageTimeout();
                } else {
                    Variables.OverlayWindow.ShowMessage(message, true);
                    SetMessageTimeout();
                }
            }
        }

        private void CalculateBalancedTeamsRank() {
            if (Variables.LobbySession) {
                Variables.OverlayWindow.ShowMessage("Calculating balanced teams...", true);
                var message = BalancingUtils.CalculateBalancedTeamsBasedOnRank();
                if (message == null) {
                    Variables.OverlayWindow.ShowMessage("Balanced teams were copied to your clipboard, you can paste them to lobby chat.", true);
                    SetMessageTimeout();
                } else {
                    Variables.OverlayWindow.ShowMessage(message, true);
                    SetMessageTimeout();
                }
            }
        }

        private void CalculateBalancedTeamsTotalGames() {
            if (Variables.LobbySession) {
                Variables.OverlayWindow.ShowMessage("Calculating balanced teams...", true);
                var message = BalancingUtils.CalculateBalancedTeamsBasedOnTotalGames();
                if (message == null) {
                    Variables.OverlayWindow.ShowMessage("Balanced teams were copied to your clipboard, you can paste them to lobby chat.", true);
                    SetMessageTimeout();
                } else {
                    Variables.OverlayWindow.ShowMessage(message, true);
                    SetMessageTimeout();
                }
            }
        }

        private void CalculateBalancedTeamsWinRatio() {
            if (Variables.LobbySession) {
                Variables.OverlayWindow.ShowMessage("Calculating balanced teams...", true);
                var message = BalancingUtils.CalculateBalancedTeamsBasedOnWinRatio();
                if (message == null) {
                    Variables.OverlayWindow.ShowMessage("Balanced teams were copied to your clipboard, you can paste them to lobby chat.", true);
                    SetMessageTimeout();
                } else {
                    Variables.OverlayWindow.ShowMessage(message, true);
                    SetMessageTimeout();
                }
            }
        }

        private void SetMessageTimeout() {
            _lastMessageId++;
            var messageId = _lastMessageId;
            Task.Delay(5000).ContinueWith(task => {
                if (_lastMessageId == messageId) {
                    Variables.OverlayWindow.HideMessage();
                }
            });
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

        void IDisposable.Dispose() {
            _repository.Dispose();
        }
    }
}
