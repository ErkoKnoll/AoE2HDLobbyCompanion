import { BasePlayer } from '../../app.models';

export interface Lobby {
    sLobbyId: string;
    name: string;
    gameType: number;
    ranked: number;
    players: Player[];
}

export interface Player extends BasePlayer {
    sSteamId: string;
    lobbySlotId: number;
    name: string;
    nameFormatted: string;
    rank: number;
    rankRM: number;
    rankDM: number;
    position: number;
    profileDataFetched: Date;
    gameStatsNormalized: PlayerGameStatsNormalized;
}

export interface PlayerGameStatsNormalized {
    totalGames: number;
    winRatio: number;
    dropRatio: number;
}