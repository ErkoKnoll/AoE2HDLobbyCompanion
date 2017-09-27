export interface BaseCommand {
    id: number;
}

export interface LogCommand extends BaseCommand {
    log: Log;
}

export interface UpdateAvailableCommand extends BaseCommand {
    version: Version;
}

export interface Log {
    message: string;
    payload: string;
    level: LogLevel;
}

export interface Version {
    downloadUrl: string;
}

export enum LogLevel {
    INFO = 0,
    WARN = 1,
    ERROR = 2
}

export interface Configuration {
    clientId: string;
    showOverlay: boolean;
    showOverlayWhenFocused: boolean;
    showOverlayWhenInLobby: boolean;
    steamApiKey: string;
    tosAccepted: boolean;
    skipSessionHelp: boolean;
    cmdPath: string;
    overlayActiveTopPosition: number;
    overlayActiveLeftPosition: number;
    overlayInactiveTopPosition: number;
    overlayInactiveLeftPosition: number;
}

export interface BasePlayer {
    fieldColors: { [key: string]: number };
    gameStats: PlayerGameStats;
    profile: PlayerProfile;
    reputatonStats: PlayerReputationStats;
}

export interface PlayerGameStats {
    totalGamesRM: number;
    winRatioRM: number;
    dropRatioRM: number;
    totalGamesDM: number;
    winRatioDM: number;
    dropRatioDM: number;
}

export interface PlayerProfile {
    profilePrivate: boolean;
    location: string;
}

export interface PlayerReputationStats {
    games: number;
    negativeReputation: number;
    positiveReputation: number;
}

export interface User extends BasePlayer {
    id: string;
    sSteamId: string;
    name: string;
    location: string;
    games: number;
    rankRM: number;
    rankDM: number;
    positiveReputaton: number;
    negativeReputation: number;
    gamesStartedRM: number;
    gamesEndedRM: number;
    gamesWonRM: number;
    gamesStartedDM: number;
    gamesEndedDM: number;
    gamesWonDM: number;
    profilePrivate: boolean;
    profileDataFetched: string;
    knownNames: string[];
    reputations: UserReputation[];
}

export interface UserReputation {
    id: number;
    comment: string;
    added: string;
    reputation: Reputation;
    lobby: Lobby;
    user: User;
}

export interface Lobby {
    id: number;
    lobbyId: string;
    name: string;
}

export interface Reputation {
    id: number;
    name: string;
    type: ReputationType;
    commentRequired: boolean;
    orderSequence: number;
}

export interface MatchHistory {
    id: number;
    name: string;
    players: number;
    negativeReputations: number;
    positiveReputations: number;
    joined: string;
    started: boolean;
    lobbySlots: MatchHistoryLobbySlot[];
    reputations: UserReputation[];
}

export interface MatchHistoryLobbySlot {
    sSteamId: string;
    name: string;
    position: number;
    rank: number;
    totalGames: number;
    winRatio: number;
    dropRatip: number;
    profilePrivate: boolean;
}

export enum ReputationType {
    NEGATIVE = 0,
    POSITIVE = 1
}