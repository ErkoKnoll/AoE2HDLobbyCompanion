import { Player } from '../../app.models';

export interface Lobby {
    sLobbyId: string;
    name: string;
    gameType: number;
    ranked: number;
    players: Player[];
}
