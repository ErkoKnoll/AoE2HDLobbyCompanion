import { Component, Inject, ViewChild, OnInit } from '@angular/core';
import { MD_DIALOG_DATA, MdDialogRef, MdDialog, MdPaginator } from '@angular/material';
import { DataSource } from '@angular/cdk';

import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/startWith';
import 'rxjs/add/observable/merge';
import 'rxjs/add/operator/map';

import { AppService, ReputationService, TrackingService, UserProfileDialogComponent, UserProfileDialogData, ConfirmationDialogComponent, ConfirmationDialogData } from '../../shared';
import { Reputation, UserReputation } from '../../app.models';

@Component({
    selector: 'reputation-details-dialog',
    templateUrl: './reputation-details-dialog.component.html',
    styleUrls: ['./reputation-details-dialog.component.scss']
})
export class ReputationDetailsDialogComponent implements OnInit {
    public loading = true;
    public userReputationsDataSource: UserReputationsDataSource;
    public displayedColumns = ["lobbyName", "player", "added", "comment", "actions"];
    @ViewChild(MdPaginator) paginator: MdPaginator;

    constructor( @Inject(MD_DIALOG_DATA) public data: ReputationDetailsDialogData, private appService: AppService, private reputationService: ReputationService, private trackingService: TrackingService, private dialog: MdDialogRef<ReputationDetailsDialogComponent>, private dialogController: MdDialog) {
    }

    public ngOnInit() {
        this.getReputations();
    }

    private getReputations() {
        this.loading = true;
        this.reputationService.getUserReputationsForReputationType(this.data.reputation.id).subscribe(userReputations => {
            this.userReputationsDataSource = new UserReputationsDataSource(userReputations, this.paginator);
            this.loading = false;
        }, error => {
            console.error("Failed to fetch reputation details", error);
            this.dialog.close();
            this.appService.toastError("Failed to open reputation details.");
        });
    }

    public openUserProfileDialog(reputation: UserReputation) {
        let dialog = this.dialogController.open(UserProfileDialogComponent, {
            data: <UserProfileDialogData>{
                steamId: reputation.user.sSteamId
            },
            width: window.innerWidth * 0.75 + "px",
        });
        this.trackingService.sendEvent("Reputations", "OpenUserProfile");
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
                this.loading = true;
                this.reputationService.deleteReputation(reputation.id).subscribe(() => {
                    this.getReputations();
                    this.data.reputationDeleted();
                    this.appService.toastSuccess("Reputation deleted.");
                    this.trackingService.sendEvent("ReputationTypeDetailsDialog", "DeleteReputation");
                }, error => {
                    this.dialog.close();
                    console.log("Error occurred while deleting reputation", error);
                    this.appService.toastError("Failed to delete reputation. Check the logs.");
                });
            }
        });
    }
}

export class UserReputationsDataSource extends DataSource<UserReputation> {
    public userReputations: BehaviorSubject<UserReputation[]>;

    constructor(userReputations: UserReputation[], private paginator: MdPaginator) {
        super();
        this.userReputations = new BehaviorSubject<UserReputation[]>(userReputations);
    }

    public connect() {
        let displayDataChanges = [
            this.userReputations,
            this.paginator.page,
        ];

        return Observable.merge(...displayDataChanges).map(() => {
            let data = this.userReputations.value.slice();
            let startIndex = this.paginator.pageIndex * this.paginator.pageSize;
            return data.splice(startIndex, this.paginator.pageSize);
        });
    }

    public disconnect() {
    }
}


export interface ReputationDetailsDialogData {
    reputation: Reputation;
    reputationDeleted: () => void;
}