import { Component, Inject, OnInit } from '@angular/core';
import { MD_DIALOG_DATA, MdDialogRef, MdDialog } from '@angular/material';
import { DataSource, CollectionViewer } from '@angular/cdk';

import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import { MatchHistory, MatchHistoryLobbySlot, UserReputation } from '../../app.models';
import { HttpService, AppService, TrackingService, ReputationService, UserProfileDialogComponent, UserProfileDialogData, ConfirmationDialogComponent, ConfirmationDialogData } from '../';

@Component({
    selector: 'match-details-dialog',
    templateUrl: './match-details-dialog.component.html',
    styleUrls: ['./match-details-dialog.component.scss']
})
export class MatchDetailsDialogComponent implements OnInit {
    public match: MatchHistory;
    public playersDataSource: PlayersDataSource;
    public reputationsDataSource: ReputationsDataSource;
    public playersDisplayedColumns = ["name", "rank", "totalGames", "winRatio", "dropRatio"];
    public reputationsDisplayedColumns = ["player", "reputation", "comment", "actions"];

    constructor( @Inject(MD_DIALOG_DATA) private data: MatchDetailsDialogData, private appService: AppService, private httpService: HttpService, private trackingService: TrackingService, private reputationService: ReputationService, private dialog: MdDialogRef<MatchDetailsDialogComponent>, private dialogController: MdDialog) {
    }

    public ngOnInit() {
        this.fetchMatchDetails();
    }

    public openUserProfileDialog(player: MatchHistoryLobbySlot) {
        let dialog = this.dialogController.open(UserProfileDialogComponent, {
            data: <UserProfileDialogData>{
                steamId: player.sSteamId
            },
            width: window.innerWidth * 0.75 + "px",
        });
        this.trackingService.sendEvent("MatchDetails", "OpenUserProfile");
    }

    public openReputationDetailsDialog(reputation: UserReputation) {
        console.log(reputation);
        this.appService.openReputationDetailsDialog("UserProfileDialog", reputation.reputation, () => this.fetchMatchDetails());
    }

    public deleteReputation(reputation: UserReputation) {
        let dialog = this.dialogController.open(ConfirmationDialogComponent, {
            data: <ConfirmationDialogData>{
                title: "Deleting Reputation",
                question: "Are you sure you want to delete assigned reputation?"
            }
        });
        dialog.afterClosed().subscribe((confirmation: boolean) => {
            if (confirmation) {
                this.match = null;
                this.reputationService.deleteReputation(reputation.id).subscribe(() => {
                    this.fetchMatchDetails();
                    if (this.data.reputationDeleted) {
                        this.data.reputationDeleted();
                    }
                    this.appService.toastSuccess("Reputation deleted.");
                    this.trackingService.sendEvent("UserProfileDialog", "DeleteReputation");
                }, error => {
                    this.dialog.close();
                    console.log("Error occurred while deleting reputation", error);
                    this.appService.toastError("Failed to delete reputation. Check the logs.");
                });
            }
        });
    }

    private fetchMatchDetails() {
        this.httpService.get<MatchHistory>("/api/matchHistory/" + this.data.id).subscribe(response => {
            this.match = response;
            this.playersDataSource = new PlayersDataSource(response.lobbySlots);
            this.reputationsDataSource = new ReputationsDataSource(response.reputations);
        }, error => {
            console.error("Failed to fetch match details for: " + this.data.id, error);
            this.dialog.close();
            this.appService.toastError("Failed to open match details.");
        });
    }
}

export class PlayersDataSource extends DataSource<MatchHistoryLobbySlot> {
    public players: BehaviorSubject<MatchHistoryLobbySlot[]>;

    constructor(players: MatchHistoryLobbySlot[]) {
        super();
        this.players = new BehaviorSubject<MatchHistoryLobbySlot[]>(players);
    }

    public connect(collectionViewer: CollectionViewer) {
        return this.players.asObservable();
    }

    public disconnect(collectionViewer: CollectionViewer) {
    }
}

export class ReputationsDataSource extends DataSource<UserReputation> {
    public reputations: BehaviorSubject<UserReputation[]>;

    constructor(reputations: UserReputation[]) {
        super();
        this.reputations = new BehaviorSubject<UserReputation[]>(reputations);
    }

    public connect(collectionViewer: CollectionViewer) {
        return this.reputations.asObservable();
    }

    public disconnect(collectionViewer: CollectionViewer) {
    }
}

export interface MatchDetailsDialogData {
    id: number;
    reputationDeleted?(): void;
}