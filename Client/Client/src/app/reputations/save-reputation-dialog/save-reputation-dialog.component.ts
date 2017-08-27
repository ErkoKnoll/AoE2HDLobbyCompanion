import { Component, Inject } from '@angular/core';
import { MD_DIALOG_DATA, MdDialogRef } from '@angular/material';

import { AppService, ReputationService, Reputation, ReputationType } from '../../shared';

@Component({
    selector: 'save-reputation-dialog',
    templateUrl: './save-reputation-dialog.component.html',
})
export class SaveReputationDialogComponent {
    public loading = false;
    public name = "";
    public reputationType = "0";
    public commentRequired = false;

    constructor( @Inject(MD_DIALOG_DATA) public data: SaveReputationDialogData, private appService: AppService, private reputationService: ReputationService, private dialog: MdDialogRef<SaveReputationDialogComponent>) {
        if (data.reputation) {
            this.name = data.reputation.name;
            this.reputationType = data.reputation.type + "";
            this.commentRequired = data.reputation.commentRequired;
        }
    }

    public save() {
        if (this.name.trim().length == 0) {
            this.appService.toastError("Name is required.");
            return;
        } else if (this.name.trim().length > 50) {
            this.appService.toastError("Name length must not exceed 50 characters.");
            return;
        }
        if (!this.data.reputation) {
            if (this.data.existingReputations.find(r => r.type == parseInt(this.reputationType) && r.name.trim().toLowerCase() == this.name.trim().toLowerCase())) {
                this.appService.toastError("Reputation with such name already exists.");
                return;
            }
        } else {
            if (this.data.existingReputations.find(r => r.id != this.data.reputation.id && r.type == parseInt(this.reputationType) && r.name.trim().toLowerCase() == this.name.trim().toLowerCase())) {
                this.appService.toastError("Reputation with such name already exists.");
                return;
            }
        }
        this.loading = true;
        if (!this.data.reputation) {
            this.reputationService.addReputation({
                name: this.name.trim(),
                reputationType: parseInt(this.reputationType),
                commentRequired: this.commentRequired
            }).subscribe(() => {
                this.appService.toastSuccess("Reputation added.");
                this.reputationService.fetchReputations(ReputationType.NEGATIVE);
                this.dialog.close(true);
            }, error => {
                this.loading = false;
                console.log("Failed to add reputation", error);
                this.appService.toastError("Failed to add reputation, check the logs.");
            });
        } else {
            this.reputationService.saveReputation(this.data.reputation.id, {
                name: this.name.trim(),
                reputationType: parseInt(this.reputationType),
                commentRequired: this.commentRequired,
                orderSequence: this.data.reputation.orderSequence
            }).subscribe(() => {
                this.appService.toastSuccess("Reputation saved.");
                this.reputationService.fetchReputations(ReputationType.NEGATIVE);
                this.dialog.close(true);
            }, error => {
                this.loading = false;
                console.log("Failed to save reputation", error);
                this.appService.toastError("Failed to save reputation, check the logs.");
            });
        }
    }
}

export interface SaveReputationDialogData {
    existingReputations: Reputation[];
    reputation: Reputation;
}