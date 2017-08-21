import { Component, Inject, OnInit } from '@angular/core';
import { MD_DIALOG_DATA, MdDialogRef } from '@angular/material';

import { AppService, ReputationService, TrackingService, Reputation, ReputationType } from '../';

@Component({
    selector: 'assign-reputation-dialog',
    templateUrl: './assign-reputation-dialog.component.html',
})
export class AssignReputationDialogComponent implements OnInit {
    public loading = true;
    public playerName: string;
    public reputationType: ReputationType;
    public reputation: number;
    public comment = "";
    public reputationList: Reputation[];
    public selectedReputationId: number;
    public selectedReputation: Reputation;

    constructor( @Inject(MD_DIALOG_DATA) private data: AssignReputationDialogData, private appService: AppService, private reputationService: ReputationService, private trackingService: TrackingService, private dialog: MdDialogRef<AssignReputationDialogComponent>) {
        this.playerName = data.playerName;
        this.reputationType = data.reputationType;
    }

    public ngOnInit() {
        this.reputationService.getReputations(this.reputationType).then(reputations => {
            this.reputationList = reputations;
            this.selectedReputation = this.reputationList[0];
            this.selectedReputationId = this.reputationList[0].id;
            this.loading = false;
        }, error => console.error("Failed to get reputation list", error));
    }

    public reputationValueChange() {
        this.selectedReputation = this.reputationList.find(r => r.id == this.selectedReputationId);
    }

    public save() {
        if (this.selectedReputation.commentRequired && this.comment.trim().length == 0) {
            this.appService.toastError("Comment is required!");
        } else {
            this.loading = true;
            this.reputationService.assignReputation({
                playerSteamId: this.data.playerSteamId,
                lobbyId: this.data.lobbyId,
                lobbySlotId: this.data.lobbySlotId,
                reputationId: this.selectedReputationId,
                comment: this.comment
            }).subscribe(() => {
                this.dialog.close(true);
                this.appService.toastSuccess("Reputation assigned.");
                if (this.reputationType == ReputationType.NEGATIVE) {
                    this.trackingService.sendEvent("AssignReputationDialog", "NegativeReputationAssigned");
                } else {
                    this.trackingService.sendEvent("AssignReputationDialog", "PositiveReputationAssigned");
                }
            }, error => {
                this.loading = false;
                console.log("Error occurred while assining reputation", error);
                this.appService.toastError("Failed to assign reputation. Check the logs.");
            });
        }
    }
}

export interface AssignReputationDialogData {
    playerSteamId: string;
    playerName: string;
    reputationType: ReputationType;
    lobbyId: string;
    lobbySlotId: number;
}