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
    steamApiKey: string;
    tosAccepted: boolean;
    skipSessionHelp: boolean;
    cmdPath: string;
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