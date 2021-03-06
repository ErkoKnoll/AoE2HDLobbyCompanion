﻿import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { MdDialog } from '@angular/material';
import { DataSource, CollectionViewer } from '@angular/cdk';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subscription } from 'rxjs/Subscription';

import { ConfirmationDialogComponent, ConfirmationDialogData, AssignReputationDialogComponent, AssignReputationDialogData, UserProfileDialogComponent, UserProfileDialogData, CommandService, Commands, AppService, TrackingService } from '../../shared';
import { ReputationType, Player } from '../../app.models';
import { LobbyService, Lobby } from '../shared';
import { MoreOptionsDialogComponent } from '../more-options-dialog';

@Component({
    selector: 'running-session-page',
    templateUrl: 'running-session-page.component.html',
    styleUrls: ['./running-session-page.component.scss']
})
export class RunningSessionPageComponent implements OnInit, OnDestroy {
    @Output() sessionStop = new EventEmitter<any>();
    @Output() lobbySealed = new EventEmitter<any>();
    @Input() lobbyId: string;
    public loading = false;
    public lobby: Lobby;
    public lobbyStarted = false;
    public displayedColumns = ["name", "rank", "location", "games", "negativeRep", "positiveRep", "totalGames", "winRatio", "dropRatio"];
    public lobbyPlayersDataSource: LobbyPlayersDataSource;
    private lobbyFetcherInterval: NodeJS.Timer;
    private subscriptions: Subscription[] = [];

    constructor(private appService: AppService, private lobbyService: LobbyService, private commandService: CommandService, private trackingService: TrackingService, private dialog: MdDialog) {
    }

    public ngOnInit() {
        this.lobbyPlayersDataSource = new LobbyPlayersDataSource();
        this.startLobbyFetcherTimer();
        if (this.lobbyId == "0") {
            this.subscriptions.push(this.appService.Events.gameStarted.subscribe(() => {
                this.lobbyId = this.lobby.sLobbyId;
                this.lobbyStarted = true;
                this.appService.stopNethook(() => { });
                this.appService.toastInfo("Lobby started!");
            }));
        } else {
            this.lobbyStarted = true;
        }
    }

    public startLobbyFetcherTimer() {
        this.lobbyFetcherInterval = setInterval(() => {
            this.lobbyService.getLobby(this.lobbyId).subscribe(lobby => {
                if (lobby) {
                    if (JSON.stringify(this.lobby) != JSON.stringify(lobby)) {
                        this.lobby = lobby;
                        this.lobbyPlayersDataSource.players.next(lobby.players);
                    }
                }
            });
        }, 500);
    }

    public stopSession() {
        this.sessionStop.emit();
    }

    public startLobby() {
        let dialog = this.dialog.open(ConfirmationDialogComponent, {
            data: <ConfirmationDialogData>{
                title: "Starting lobby",
                question: "Are you sure you want to start the lobby? If you won't the lobby will be started automatically anyway after 1 minute when the game has started."
            }
        });
        dialog.afterClosed().subscribe((confirmation: boolean) => {
            if (confirmation) {
                this.commandService.sendCommand(Commands.OUT.START_LOBBY_MANUALLY).subscribe();
                this.trackingService.sendEvent("Lobby", "LobbyStartedManually");
            }
        });
    }

    public openNegativeReputationAssingmentDialog(player: Player) {
        this.openReputationAssignmentDialog(player, ReputationType.NEGATIVE);
        this.trackingService.sendEvent("Lobby", "OpenNegativeAssignmentDialog");
    }

    public openPositiveReputationAssingmentDialog(player: Player) {
        this.openReputationAssignmentDialog(player, ReputationType.POSITIVE);
        this.trackingService.sendEvent("Lobby", "OpenPositiveAssignmentDialog");
    }

    private openReputationAssignmentDialog(player: Player, reputationType: ReputationType) {
        let dialog = this.dialog.open(AssignReputationDialogComponent, {
            data: <AssignReputationDialogData>{
                playerSteamId: player.sSteamId,
                playerName: player.name,
                reputationType: reputationType,
                lobbyId: this.lobby.sLobbyId,
                lobbySlotId: player.lobbySlotId
            }
        });
    }

    public openUserProfileDialog(player: Player) {
        let dialog = this.dialog.open(UserProfileDialogComponent, {
            data: <UserProfileDialogData>{
                steamId: player.sSteamId
            },
            width: window.innerWidth * 0.75 + "px",
        });
        this.trackingService.sendEvent("Lobby", "OpenUserProfile");
    }

    public openMoreOptions() {
        this.dialog.open(MoreOptionsDialogComponent);
    }

    public sealLobby() {
        let dialog = this.dialog.open(ConfirmationDialogComponent, {
            data: <ConfirmationDialogData>{
                title: "Sealing lobby",
                question: "Are you sure you want to seal the lobby?"
            }
        });
        dialog.afterClosed().subscribe((confirmation: boolean) => {
            if (confirmation) {
                this.loading = true;
                this.commandService.sendCommand(Commands.OUT.SEAL_LOBBY, this.lobby.sLobbyId).subscribe(() => {
                    this.lobbySealed.emit();
                    this.appService.toastSuccess("Lobby sealed!");
                    this.trackingService.sendEvent("Lobby", "LobbySealed");
                }, error => {
                    this.loading = false;
                    console.error("Failed to seal the lobby", error);
                    this.appService.toastError("Failed to seal the lobby.");
                });
            }
        });
    }

    public calculateBalancedTeamsBasedOnRank() {
        this.calculateBalancedTeams(Commands.OUT.CALCULATE_BALANCED_TEAMS_BASED_ON_RANK);
    }

    public calculateBalancedTeamsBasedOnTotalGames() {
        this.calculateBalancedTeams(Commands.OUT.CALCULATE_BALANCED_TEAMS_BASED_ON_TOTAL_GAMES);
    }

    public calculateBalancedTeamsBasedOnWinRatio() {
        this.calculateBalancedTeams(Commands.OUT.CALCULATE_BALANCED_TEAMS_BASED_ON_WIN_RATIO);
    }

    public copyPlayerStats() {
        this.commandService.sendCommandAndReadRawResponse(Commands.OUT.COPY_PLAYER_STATS).subscribe(response => {
            if (response.text()) {
                this.appService.toastError(response.text());
            } else {
                this.appService.toastSuccess("Player stats were copied to your clipboard, you can paste them to lobby chat.");
            }
        }, error => {
            console.log("Failed to copy player stats", error);
            this.appService.toastError("Failed to copy player stats.");
        });
    }

    private calculateBalancedTeams(command: Commands.OUT) {
        this.commandService.sendCommandAndReadRawResponse(command).subscribe(response => {
            if (response.text()) {
                this.appService.toastError(response.text());
            } else {
                this.appService.toastSuccess("Balanced teams were copied to your clipboard, you can paste them to lobby chat.");
            }
        }, error => {
            console.log("Failed to calculate balanced teams", error);
            this.appService.toastError("Failed to calculate balanced teams.");
        });
    }

    public ngOnDestroy() {
        if (this.lobbyFetcherInterval) {
            clearInterval(this.lobbyFetcherInterval);
        }
        this.subscriptions.forEach(subscription => subscription.unsubscribe());
    }
}

export class LobbyPlayersDataSource extends DataSource<Player> {
    public players = new BehaviorSubject<Player[]>([]);

    public connect(collectionViewer: CollectionViewer) {
        return this.players.asObservable();
    }

    public disconnect(collectionViewer: CollectionViewer) {
    }
}
