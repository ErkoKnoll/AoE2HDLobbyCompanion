import { Component, Inject, OnInit } from '@angular/core';
import { MD_DIALOG_DATA, MdDialogRef } from '@angular/material';

import { AppService, ReputationService, Reputation, ReputationType } from '../../shared';

@Component({
    selector: 'delete-reputation-dialog',
    templateUrl: './delete-reputation-dialog.component.html',
})
export class DeleteReputationDialogComponent implements OnInit {
    public loading = true;
    public allReputations: Reputation[] = [];
    public migrateReputations = false;
    public selectedReputationId = 0;

    constructor( @Inject(MD_DIALOG_DATA) public data: Reputation, private appService: AppService, private reputationService: ReputationService, private dialog: MdDialogRef<DeleteReputationDialogComponent>) {
    }

    public ngOnInit() {
        this.reputationService.getNegativeReputations().then(negativeReputations => {
            negativeReputations.forEach(reputation => {
                this.allReputations.push({
                    id: reputation.id,
                    name: "[Negative] " + reputation.name,
                    commentRequired: reputation.commentRequired,
                    orderSequence: reputation.orderSequence,
                    type: reputation.type
                })
            })
            this.reputationService.getPositiveReputations().then(positiveReputations => {
                positiveReputations.forEach(reputation => {
                    this.allReputations.push({
                        id: reputation.id,
                        name: "[Positive] " + reputation.name,
                        commentRequired: reputation.commentRequired,
                        orderSequence: reputation.orderSequence,
                        type: reputation.type
                    })
                })
            });
        });
        this.loading = false;
    }

    public delete() {
        if (this.migrateReputations && this.selectedReputationId == 0) {
            this.appService.toastError("Please select Migrate Existing Reputations To value.")
            return;
        } else {
            this.loading = true;
            this.reputationService.deleteReputationType(this.data.id, this.migrateReputations ? this.selectedReputationId : 0).subscribe(() => {
                this.dialog.close(true);
                this.reputationService.fetchReputations(ReputationType.NEGATIVE);
                this.appService.toastSuccess("Reputation type deleted.");
            }, error => {
                this.loading = false;
                console.error("Failed to delete reputation type", error);
                this.appService.toastError("Failed to delete reputation type, check the logs");
            });
        }
    }
}