import { Injectable } from '@angular/core';
import { MdDialog } from '@angular/material';

import { AppService, HttpService, TrackingService, UpdateAvailableComponent } from './';
import { BaseCommand, LogCommand, UpdateAvailableCommand } from '../app.models';

@Injectable()
export class CommandService {
    private firstSuccessNotified = false;

    constructor(private appService: AppService, private httpService: HttpService, private trackingService: TrackingService, private dialog: MdDialog) {
    }

    public sendCommand(id: Commands.OUT, payload?: any) {
        return this.httpService.put("/api/commands/" + id, payload);
    }

    public sendCommandAndReadResponse<T>(id: Commands.OUT, payload?: any) {
        return this.httpService.putAndReadResponse<T>("/api/commands/" + id, payload);
    }

    public sendCommandAndReadRawResponse<T>(id: Commands.OUT, payload?: any) {
        return this.httpService.putAndReadRawResponse<T>("/api/commands/" + id, payload);
    }

    public startListeningIncomingCommands(firstSuccess: () => void) {
        setInterval(() => {
            try {
                this.httpService.get<BaseCommand[]>("/api/commands").subscribe(commands => {
                    if (!this.firstSuccessNotified) {
                        this.firstSuccessNotified = true;
                        firstSuccess();
                    }
                    if (commands) {
                        commands.forEach(command => {
                            try {
                                switch (command.id) {
                                    case Commands.IN.GAME_STARTED:
                                        this.processGameStarted();
                                        break;
                                    case Commands.IN.WRITE_LOG:
                                        this.processWriteLog(<LogCommand>command);
                                        break;
                                    case Commands.IN.UPDATE_AVAILABLE:
                                        this.processUpdateAvailable(<UpdateAvailableCommand>command);
                                        break;
                                }
                            } catch (e) {
                                console.error("Error while processing incoming command: " + command.id);
                            }
                        })
                    }
                });
            } catch (e) {
                console.error("Error while running commands fetcher", e);
            }
        }, 500);
    }

    private processGameStarted() {
        this.appService.sessionRunning = false;
        this.appService.Events.gameStarted.next();
    }

    private processWriteLog(command: LogCommand) {
        switch (command.log.level) {
            case 0:
                console.info(command.log.message);
                break;
            case 1:
                console.warn(command.log.message);
                break;
            case 2:
                if (command.log.payload) {
                    console.error(command.log.message, command.log.payload);
                    this.trackingService.logException(command.log.message + " " + command.log.payload);
                } else {
                    console.error(command.log.message);
                    this.trackingService.logException(command.log.message);
                }
                break;

        }
    }

    private processUpdateAvailable(command: UpdateAvailableCommand) {
        this.dialog.open(UpdateAvailableComponent, {
            data: command.version
        });
    }
}

export module Commands {
    export enum IN {
        GAME_STARTED = 0,
        WRITE_LOG = 1,
        UPDATE_AVAILABLE = 2
    }
    export enum OUT {
        LOBBY_SESSION_START = 0,
        LOBBY_SESSION_STOP = 1,
        SET_UP = 2,
        START_LOBBY_MANUALLY = 3,
        SEAL_LOBBY = 4,
        GET_UNSEALED_LOBBY = 5,
        CALCULATE_BALANCED_TEAMS_BASED_ON_RANK = 6,
        CALCULATE_BALANCED_TEAMS_BASED_ON_TOTAL_GAMES = 7,
        CALCULATE_BALANCED_TEAMS_BASED_ON_WIN_RATIO = 8,
        COPY_PLAYER_STATS = 9
    }
}