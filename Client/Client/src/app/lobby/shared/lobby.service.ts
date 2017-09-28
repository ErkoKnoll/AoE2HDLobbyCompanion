import { Injectable } from '@angular/core';

import 'rxjs/add/operator/map'

import { HttpService, CommandService, Commands } from '../../shared';
import { Lobby } from './';

@Injectable()
export class LobbyService {

    constructor(private httpService: HttpService, private commandService: CommandService) {
    }

    public getLobby(lobbyId: string) {
        return this.httpService.get<Lobby>("/api/lobby/" + lobbyId).map(lobby => {
            if (lobby) {
                this.normalizeLobbyData(lobby);
            }
            return lobby;
        });
    }

    public normalizeLobbyData(lobby: Lobby) {
        lobby.players.forEach(player => {
            if (player.gameStats) {
                if (lobby.ranked == 2) {
                    player.gameStatsNormalized = {
                        totalGames: player.gameStats.totalGamesDM,
                        winRatio: player.gameStats.winRatioDM,
                        dropRatio: player.gameStats.dropRatioDM
                    }
                } else {
                    player.gameStatsNormalized = {
                        totalGames: player.gameStats.totalGamesRM,
                        winRatio: player.gameStats.winRatioRM,
                        dropRatio: player.gameStats.dropRatioRM
                    }
                }
            }
        });
    }

    public startSession(done: (success: boolean) => void) {
        this.commandService.sendCommand(Commands.OUT.LOBBY_SESSION_START).subscribe(() => done(true), () => done(false));
    }

    public stopSession(done: (success: boolean) => void) {
        this.commandService.sendCommand(Commands.OUT.LOBBY_SESSION_STOP).subscribe(() => done(true), () => done(false));
    }
}
