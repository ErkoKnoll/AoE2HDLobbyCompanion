using Backend.Global;
using Backend.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Backend.Jobs {
    public class OverlayUpdaterJob {
        private Timer _job;
        private bool _jobRunning = false;

        public OverlayUpdaterJob() {
            StartJob();
        }

        private void StartJob() {
            _job = new Timer(state => {
                if (!_jobRunning) {
                    _jobRunning = true;
                    try {
                        if (Variables.OverlayWindow != null) {
                            lock (Variables.LobbyPlayers) {
                                LobbyUtils.CalculateLobbyPlayerFieldColors();
                                Variables.OverlayWindow.UpdatePlayers(Variables.LobbyPlayers, Variables.Teams, Variables.Lobby.Ranked == 2);
                            }
                        }
                    } catch (Exception e) {
                        Console.WriteLine("Error while running overlay updater job");
                        Console.WriteLine(e.ToString());
                    } finally {
                        _jobRunning = false;
                    }
                }
            }, null, 0, 500);
        }
    }
}
