using Backend.Constants;
using Backend.Global;
using Backend.Utils;
using Commons.Constants;
using Commons.Models;
using Commons.Models.Commands;
using Database;
using Newtonsoft.Json;
using SteamKit2;
using SteamKit2.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Commons.Extensions;
using Database.Domain;
using Microsoft.EntityFrameworkCore;

namespace Backend.Jobs {
    public class NetHookDumpReaderJob {
        private Timer _job;
        private bool _jobRunning = false;

        public NetHookDumpReaderJob() {
            LobbyUtils.ResetLobbyData();
            if (Variables.ReplayMode) {
                ReadNewFiles();
            } else {
                StartJob();
            }
        }

        private void StartJob() {
            _job = new Timer(state => {
                if (!_jobRunning && Variables.LobbySession) {
                    _jobRunning = true;
                    try {
                        ReadNewFiles();
                    } catch (Exception e) {
                        Console.WriteLine("Error while running dump reader job");
                        Console.WriteLine(e.ToString());
                    } finally {
                        _jobRunning = false;
                    }
                }
            }, null, 0, 100);
        }

        private void ReadNewFiles() {
            DirectoryInfo info = new DirectoryInfo(Variables.NethookDumpDir);
            var files = info.GetFiles().OrderBy(p => p.CreationTime).ToList();
            foreach (var file in files) {
                try {
                    if (Variables.LobbySession) {
                        try {
                            ReadFile(file);
                        } catch {
                            Thread.Sleep(25);
                            ReadFile(file); //Try reading again in 25ms if the first time file was locked.
                        }
                    }
                } catch {
                }
            }
            if (!Variables.ReplayMode) {
                foreach (var file in files) {
                    try {
                        File.Move(file.FullName, Path.Combine(Variables.NethookDumpDirParsed, file.Name));
                    } catch { }
                }
            }
        }

        private void ReadFile(FileInfo file) {
            var item = new NetHookItem();
            if (item.LoadFromFile(file)) {
                switch (item.Direction) {
                    case NetHookItem.PacketDirection.In:
                        switch (item.EMsg) {
                            case EMsg.ClientMMSLobbyData:
                                ProcessLobbyUpdate(item);
                                SetLobbyPresence(true);
                                break;
                            case EMsg.ClientMMSCreateLobbyResponse:
                                ProcessCreateLobbyResponse(item);
                                break;
                            case EMsg.ClientMMSJoinLobbyResponse:
                                ProcessJoinLobbyResponse(item);
                                break;
                        }
                        break;
                    case NetHookItem.PacketDirection.Out:
                        switch (item.EMsg) {
                            case EMsg.ClientMMSCreateLobby:
                            case EMsg.ClientMMSJoinLobby:
                                LobbyUtils.ResetLobbyData();
                                SetLobbyPresence(true);
                                ClearOverlayTeams();
                                break;
                            case EMsg.ClientMMSLeaveLobby:
                                ProcessLobbyLeave();
                                SetLobbyPresence(false);
                                break;
                            case EMsg.ClientLBSSetScore:
                                ProcessLeaderBoardUpdate(item);
                                break;
                        }
                        break;
                }
            } else {
                throw new Exception("Failed to read");
            }
        }

        private void SetLobbyPresence(bool inLobby) {
            if (Variables.OverlayWindow != null) {
                Variables.OverlayWindow.UpdateLobbyPresence(inLobby);
            }
        }

        private void ProcessCreateLobbyResponse(NetHookItem item) {
            var msg = item.ReadAsProtobufMsg<CMsgClientMMSCreateLobbyResponse>();
            Variables.Lobby.LobbyId = msg.Body.steam_id_lobby;
            CheckIfReJoiningStartedLobby();
        }

        private void ProcessJoinLobbyResponse(NetHookItem item) {
            var msg = item.ReadAsProtobufMsg<CMsgClientMMSJoinLobbyResponse>();
            Variables.Lobby.LobbyId = msg.Body.steam_id_lobby;
            if (msg.Body.chat_room_enter_response != 2) {
                CheckIfReJoiningStartedLobby();
            } else { //Lobby is not joinable any more
                SetLobbyPresence(false);
            }
        }

        private void CheckIfReJoiningStartedLobby() {
            try {
                using (IRepository repository = new Repository()) {
                    var lobby = repository.Lobbies.Include(l => l.Players).ThenInclude(p => p.User).FirstOrDefault(l => l.LobbyId == Variables.Lobby.LobbyId && l.Started.HasValue);
                    if (lobby != null) {
                        lobby.Joined = DateTime.UtcNow;
                        lobby.Started = null;
                        lobby.Sealed = false;
                        repository.SetModified(lobby);
                        repository.SaveChanges();
                        foreach (var player in lobby.Players.Where(p => p.Position > 0 && p.User != null)) {
                            try {
                                player.User.Games--;
                                repository.SetModified(player.User);
                                repository.SaveChanges();
                            } catch (Exception e) {
                                LogUtils.Error("Error while modifing user for re-joined lobby: "+player.User.Name, e);
                            }
                        }
                    }
                }
            } catch (Exception e) {
                LogUtils.Error("Error while checking if re-joining started lobby", e);
            }
        }

        private void ProcessLeaderBoardUpdate(NetHookItem item) {
            var msg = item.ReadAsProtobufMsg<CMsgClientLBSSetScore>();
            if (msg.Body.leaderboard_id == 180533) {
                LobbyUtils.StartLobby(Variables.Lobby);
            }
        }

        private void ProcessLobbyLeave() {
            if (Variables.OverlayWindow != null) {
                Variables.OverlayWindow.ShowMessage("Overlay will close automatically in 1 minute if the game started. If you want to close the overlay right now or left the lobby then act in the Lobby Companion UI or join a new lobby.");
            }
        }

        private void ProcessLobbyUpdate(NetHookItem item) {
            var msg = item.ReadAsProtobufMsg<CMsgClientMMSLobbyData>();
            if (msg == null || msg.Body == null) {
                return;
            }
            var lobbyData = new KeyValue();
            using (var stream1 = new MemoryStream(msg.Body.metadata)) {
                if (!lobbyData.TryReadAsBinary(stream1)) {
                    throw new Exception("Failed to read lobby metadata");
                } else {
                    if (lobbyData == null || lobbyData.Children == null || lobbyData.Children.Count == 0) {
                        return;
                    }
                    TryExtractLobbyData(lobbyData);
                    if (msg.Body.members.Count > 0) {
                        this.ClearOverlayTeams();
                        var lobbyMembers = new List<Player>();
                        foreach (var playerData in msg.Body.members) {
                            var memberData = new KeyValue();
                            using (var stream2 = new MemoryStream(playerData.metadata)) {
                                try {
                                    if (memberData.TryReadAsBinary(stream2)) {
                                        var player = new Player() {
                                            SteamId = playerData.steam_id,
                                            Name = playerData.persona_name
                                        };
                                        try {
                                            player.RankRM = int.Parse(memberData.Children.SingleOrDefault(c => c.Name == "STAT_ELO_RM").Value);
                                        } catch { }
                                        try {
                                            player.RankDM = int.Parse(memberData.Children.SingleOrDefault(c => c.Name == "STAT_ELO_DM").Value);
                                        } catch { }
                                        lobbyMembers.Add(player);
                                    }
                                } catch (Exception e) {
                                    Console.WriteLine("Error while processing lobby member: " + playerData.persona_name);
                                    Console.WriteLine(e.ToString());
                                }
                            }
                        }
                        Variables.LobbyMembers = lobbyMembers;
                    }
                    if (lobbyData.Children.Any(c => c.Name == "player0_desc")) {
                        foreach (var lobbyMember in Variables.LobbyMembers) {
                            lobbyMember.Position = 0;
                        }
                        var lobbyPlayers = new List<Player>();
                        for (var i = 0; i < 8; i++) {
                            try {
                                var player = new Player() {
                                    Position = i + 1
                                };
                                TryExtractNameAndRank(player, lobbyData.Children.SingleOrDefault(c => c.Name == "player" + i + "_desc")?.Value, Variables.Lobby.Ranked > 0);
                                var lobbyMember = Variables.LobbyMembers.FirstOrDefault(lm => lm.Name == player.Name && lm.Position == 0);
                                if (lobbyMember != null) {
                                    lobbyMember.Position = i + 1;
                                    player.SteamId = lobbyMember.SteamId;
                                    if (player.Rank == 0) {
                                        if (Variables.Lobby.Ranked != 2) {
                                            player.Rank = lobbyMember.RankRM;
                                        } else {
                                            player.Rank = lobbyMember.RankDM;
                                        }
                                    }
                                    player.RankRM = lobbyMember.RankRM;
                                    player.RankDM = lobbyMember.RankDM;
                                }
                                if (player.SteamId != 0) {
                                    try {
                                        using (IRepository repository = new Repository()) {
                                            var lobbySlot = repository.LobbySlots.Include(ls => ls.User).GetLobbySlot(player.SteamId, Variables.Lobby.LobbyId).FirstOrDefault();
                                            if (lobbySlot == null) {
                                                var user = UserUtils.GetUser(repository, player);
                                                var lobby = repository.Lobbies.FirstOrDefault(l => l.LobbyId == Variables.Lobby.LobbyId);
                                                if (lobby == null) {
                                                    throw new Exception("Lobby not found in database: " + Variables.Lobby.LobbyId);
                                                }
                                                lobbySlot = new LobbySlot() {
                                                    Name = player.Name,
                                                    RankDM = player.RankDM,
                                                    RankRM = player.RankRM,
                                                    User = user,
                                                    Lobby = lobby
                                                };
                                                repository.Add(lobbySlot);
                                                repository.SaveChanges();
                                                LobbyUtils.AssignLobbySlotId(lobbySlot, player);
                                            } else {
                                                LobbyUtils.AssignLobbySlotId(lobbySlot, player);
                                            }
                                        }
                                    } catch (Exception e) {
                                        LogUtils.Error(string.Format("Failed to persist player joining to lobby event - Player: {0}({1}) - Lobby: {2}", player.Name, player.SteamId, Variables.Lobby.LobbyId), e);
                                    }
                                }
                                lobbyPlayers.Add(player);
                                UserUtils.FetchUserGameStats(player);
                                UserUtils.FetchUserProfile(player);
                                UserUtils.FetchUserReputationStats(player);
                            } catch (Exception e) {
                                Console.WriteLine("Error while processing lobby player in slot: " + i);
                                Console.WriteLine(e.ToString());
                            }
                            lock (Variables.LobbyPlayers) {
                                Variables.LobbyPlayers = lobbyPlayers;
                            }
                        }
                    }
                }
            }
            if (Variables.ReplayMode) {
                Thread.Sleep(500);
            }
        }

        private void ClearOverlayTeams() {
            Variables.Teams.Clear();
        }

        private void TryExtractNameAndRank(Player player, string lobbyName, bool rm) {
            if (lobbyName == null) {
                lobbyName = "Unknown";
            }
            if (!rm) {
                player.Name = lobbyName;
            } else {
                if (lobbyName.StartsWith("[") && lobbyName.Contains("]")) {
                    int rank;
                    int.TryParse(lobbyName.Substring(1, lobbyName.IndexOf("]") - 1), out rank);
                    player.Name = lobbyName.Substring(lobbyName.IndexOf("]") + 2);
                    player.Rank = rank;
                } else {
                    player.Name = lobbyName;
                }
            }
        }

        private void TryExtractLobbyData(KeyValue lobbyData) {
            try {
                if (!lobbyData.Children.Any(c => c.Name == "Age")) {
                    return;
                }

                Variables.Lobby.Name = GetLobbyData(lobbyData, "title_salt");
                Variables.Lobby.GameType = GetLobbyDataInt(lobbyData, "GameType");
                Variables.Lobby.Ranked = GetLobbyDataInt(lobbyData, "Ranked");
                try {
                    using (IRepository repository = new Repository()) {
                        var lobby = repository.Lobbies.FirstOrDefault(l => l.LobbyId == Variables.Lobby.LobbyId);
                        if (lobby == null) {
                            lobby = new Database.Domain.Lobby {
                                LobbyId = Variables.Lobby.LobbyId,
                                Joined = DateTime.UtcNow
                            };
                            repository.Add(lobby);
                        } else {
                            repository.SetModified(lobby);
                        }
                        lobby.Age = GetLobbyDataInt(lobbyData, "Age");
                        lobby.CheatsEnabled = GetLobbyDataInt(lobbyData, "CheatsEnabled");
                        lobby.DataSet = GetLobbyDataInt(lobbyData, "Dataset");
                        lobby.EndAge = GetLobbyDataInt(lobbyData, "EndAge");
                        lobby.GameType = Variables.Lobby.GameType;
                        lobby.LobbyElo = GetLobbyDataInt(lobbyData, "LobbyElo");
                        lobby.LatencyRegion = GetLobbyDataInt(lobbyData, "LatencyRegion");
                        lobby.MapSize = GetLobbyDataInt(lobbyData, "MapSize");
                        lobby.MapStyleType = GetLobbyDataInt(lobbyData, "MapStyleType");
                        lobby.MapType = GetLobbyDataInt(lobbyData, "MapType");
                        lobby.Name = Variables.Lobby.Name;
                        lobby.Pop = GetLobbyDataInt(lobbyData, "Pop");
                        lobby.Ranked = Variables.Lobby.Ranked;
                        lobby.Resource = GetLobbyDataInt(lobbyData, "Resource");
                        lobby.SlotsFilled = GetLobbyDataInt(lobbyData, "SlotsFilled");
                        lobby.Speed = GetLobbyDataInt(lobbyData, "Speed");
                        lobby.Victory = GetLobbyDataInt(lobbyData, "Victory");
                        repository.SaveChanges();
                    }
                } catch (Exception e) {
                    LogUtils.Error("Error while saving lobby data", e);
                }
            } catch (Exception e) {
                Console.WriteLine("Error while trying to extract lobby data");
                Console.WriteLine(e.ToString());
            }
        }

        private int GetLobbyDataInt(KeyValue lobbyData, string key) {
            return int.Parse(lobbyData.Children.SingleOrDefault(c => c.Name == key)?.Value);
        }

        private string GetLobbyData(KeyValue lobbyData, string key) {
            return lobbyData.Children.SingleOrDefault(c => c.Name == key)?.Value;
        }
    }
}
