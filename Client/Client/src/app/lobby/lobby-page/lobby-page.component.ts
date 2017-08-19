import { Component, OnInit } from '@angular/core';
import { MdDialog } from '@angular/material';

import { Subscription } from 'rxjs/Subscription';

import { AppService, CommandService, Commands, ConfigurationService, TrackingService } from '../../shared';
import { LobbyService, Lobby } from '../shared';

@Component({
    selector: 'lobby-page',
    templateUrl: './lobby-page.component.html',
    styleUrls: ['./lobby-page.component.scss']
})
export class LobbyPageComponent implements OnInit {
    public lobbyState = LobbyState.LOADING;
    public lobbyId = "0";

    constructor(private appService: AppService, private lobbyService: LobbyService, private commandService: CommandService, private configurationService: ConfigurationService, private trackingService: TrackingService, private dialog: MdDialog) {
    }

    public ngOnInit() {
        if (this.appService.sessionRunning) {
            this.lobbyState = LobbyState.SESSION_RUNNING;
        } else {
            this.checkForUnsealedLobby();
        }
    }

    public proceedHelp(skipNextTime: boolean) {
        this.startSession();
        if (skipNextTime) {
            this.configurationService.configuration.skipSessionHelp = true;
            this.configurationService.saveConfiguration().subscribe();
            this.trackingService.sendEvent("Lobby", "SkipHelpNextTimeSet");
        }
    }

    public sessionStart() {
        if (this.configurationService.configuration.skipSessionHelp) {
            this.startSession();
        } else {
            this.lobbyState = LobbyState.SESSION_HELP;
        }
    }

    private startSession() {
        this.lobbyState = LobbyState.LOADING;
        this.appService.startNethook(success => {
            if (success) {
                setTimeout(() => {
                    this.lobbyService.startSession(success => {
                        if (success) {
                            this.lobbyId = "0";
                            this.lobbyState = LobbyState.SESSION_RUNNING;
                            this.appService.sessionRunning = true;
                            this.trackingService.sendEvent("Lobby", "SessionStarted");
                        } else {
                            this.lobbyState = LobbyState.WAITING_FOR_SESSION;
                            this.appService.stopNethook(() => {
                                this.appService.toastError("Failed to start the session. API communication failed.");
                            });
                        }
                    });
                }, 1000); //Wait for NetHook2 to start up before asking Backend to start reading its directory
            } else {
                this.lobbyState = LobbyState.WAITING_FOR_SESSION;
                this.appService.toastError("Failed to start the session. NetHook2 process failed to start.");
            }
        });
    }

    public sessionStop() {
        this.lobbyState = LobbyState.LOADING;
        this.appService.stopNethook(success => {
            if (success) {
                this.lobbyService.stopSession(success => {
                    if (success) {
                        this.lobbyState = LobbyState.WAITING_FOR_SESSION;
                        this.appService.sessionRunning = false;
                        this.trackingService.sendEvent("Lobby", "SessionStopped");
                    } else {
                        this.lobbyState = LobbyState.SESSION_RUNNING;
                        this.appService.toastError("Failed to stop the session. API communication failed.")
                    }
                });
            } else {
                this.lobbyState = LobbyState.SESSION_RUNNING;
                this.appService.toastError("Failed to stop the session. NetHook2 process failed to stop.");
            }
        });
    }

    public lobbySealed() {
        this.checkForUnsealedLobby();
    }

    private checkForUnsealedLobby() {
        this.lobbyId = "0";
        this.lobbyState = LobbyState.LOADING;
        this.commandService.sendCommandAndReadResponse<Lobby>(Commands.OUT.GET_UNSEALED_LOBBY).subscribe(lobby => {
            if (lobby) {
                this.lobbyId = lobby.sLobbyId;
                this.lobbyState = LobbyState.SESSION_RUNNING;
                if (!this.appService.lobbiesPageOpened) {
                    this.appService.toastInfo(lobby.name + " is not yet sealed. Please seal the previous lobby before you can start a new one.");
                }
                this.appService.lobbiesPageOpened = true;
            } else {
                this.lobbyState = LobbyState.WAITING_FOR_SESSION;
                this.appService.lobbiesPageOpened = true;
            }
        }, error => {
            console.error("Failed to get unsealed lobbies", error);
            this.lobbyState = LobbyState.WAITING_FOR_SESSION;
            this.appService.lobbiesPageOpened = true;
        });
    }
}

enum LobbyState {
    LOADING = 0,
    WAITING_FOR_SESSION = 1,
    SESSION_RUNNING = 2,
    SESSION_HELP = 3
}