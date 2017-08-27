import { Component, Inject, OnInit } from '@angular/core';
import { MD_DIALOG_DATA, MdDialogRef, MdDialog } from '@angular/material';
import { DataSource, CollectionViewer } from '@angular/cdk';
import { shell } from 'electron';

import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import { BasePlayer } from '../../app.models';
import { HttpService, AppService, TrackingService, ConfirmationDialogComponent, ConfirmationDialogData, ReputationService, Reputation } from '../';

@Component({
    selector: 'user-profile-dialog',
    templateUrl: './user-profile-dialog.component.html',
    styleUrls: ['./user-profile-dialog.component.scss']
})
export class UserProfileDialogComponent implements OnInit {
    public profile: UserProfile;
    public knownNames: string;
    public userReputationsDataSource: UserReputationsDataSource;
    public displayedColumns = ["lobbyName", "reputationType", "added", "comment", "actions"];

    constructor( @Inject(MD_DIALOG_DATA) private data: UserProfileDialogData, private appService: AppService, private reputationService: ReputationService, private httpService: HttpService, private trackingService: TrackingService, private dialog: MdDialogRef<UserProfileDialogComponent>, private confirmationDialog: MdDialog) {
    }

    public ngOnInit() {
        this.fetchProfile();
    }

    public deleteReputation(reputation: UserReputation) {
        let dialog = this.confirmationDialog.open(ConfirmationDialogComponent, {
            data: <ConfirmationDialogData>{
                title: "Deleting Reputation",
                question: "Are you sure you want to delete assigned reputation?"
            }
        });
        dialog.afterClosed().subscribe((confirmation: boolean) => {
            if (confirmation) {
                this.profile = null;
                this.reputationService.deleteReputation(reputation.id).subscribe(() => {
                    this.fetchProfile();
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

    public openSteamProfile() {
        shell.openExternal("http://steamcommunity.com/profiles/" + this.profile.sSteamId);
        this.trackingService.sendEvent("UserProfileDialog", "SteamProfileOpened");
    }

    private fetchProfile() {
        this.httpService.get<UserProfile>("/api/userProfile/" + this.data.steamId).subscribe(response => {
            this.profile = response;
            this.userReputationsDataSource = new UserReputationsDataSource(response.reputations);
            if (this.profile.knownNames.length > 1) {
                this.knownNames = this.profile.knownNames.join(", ");
            }
        }, error => {
            console.error("Failed to fetch user profile for: " + this.data.steamId, error);
            this.dialog.close();
            this.appService.toastError("Failed to open user profile.");
        });
    }
}

export class UserReputationsDataSource extends DataSource<UserReputation> {
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

export interface UserProfileDialogData {
    steamId: string;
    lobbyId: string;
}

interface UserProfile extends BasePlayer {
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

interface UserReputation {
    id: number;
    comment: string;
    added: string;
    reputation: Reputation;
    lobby: Lobby;
}

interface Lobby {
    name: string;
}