import { Component, OnInit } from '@angular/core';
import { DataSource, CollectionViewer } from '@angular/cdk';
import { MdDialog } from '@angular/material';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import { AppService, ReputationService, ReputationWithCount, ReputationType, SaveReputationRequest } from '../../shared';
import { SaveReputationDialogComponent, SaveReputationDialogData } from '../save-reputation-dialog';
import { DeleteReputationDialogComponent } from '../delete-reputation-dialog';

@Component({
    selector: 'reputations-page',
    templateUrl: 'reputations-page.component.html',
    styleUrls: ['./reputations-page.component.scss']
})
export class ReputationsPageComponent implements OnInit {
    public loading = true;
    public negativeReputationsDataSource: ReputationsDataSource;
    public positiveReputationsDataSource: ReputationsDataSource;
    public negativeReputationsCount: number;
    public positiveReputationsCount: number;
    public displayedColumns = ["name", "commentRequired", "total", "actions"];
    private reputations: ReputationWithCount[];

    constructor(private appService: AppService, private reputationService: ReputationService, private dialog: MdDialog) {
    }

    public ngOnInit() {
        this.getReputations();
    }

    public openSaveReputationDialog(reputation: ReputationWithCount) {
        let dialog = this.dialog.open(SaveReputationDialogComponent, {
            data: <SaveReputationDialogData>{
                existingReputations: this.reputations,
                reputation: reputation
            }
        });
        dialog.afterClosed().subscribe((saved: boolean) => {
            if (saved) {
                this.getReputations();
            }
        });
    }

    public openDeleteReputationDialog(reputation: ReputationWithCount) {
        let dialog = this.dialog.open(DeleteReputationDialogComponent, {
            data: reputation
        });
        dialog.afterClosed().subscribe((saved: boolean) => {
            if (saved) {
                this.getReputations();
            }
        });
    }

    public moveUp(reputation: ReputationWithCount) {
        let previousReputation = this.reputations.find(r => r.type == reputation.type && r.orderSequence == reputation.orderSequence - 1);
        if (previousReputation) {
            previousReputation.orderSequence++;
            reputation.orderSequence--;

            this.loading = true;
            this.reputationService.saveReputation(reputation.id, this.getReputationSaveRequest(reputation)).subscribe(() => {
                this.reputationService.saveReputation(previousReputation.id, this.getReputationSaveRequest(previousReputation)).subscribe(() => {
                    this.getReputations();
                    this.reputationService.fetchReputations(ReputationType.NEGATIVE);
                }, error => {
                    console.error("Failed to move reputation", error);
                    this.appService.toastError("Failed to move reputation, check the logs.");
                });
            }, error => {
                console.error("Failed to move reputation", error);
                this.appService.toastError("Failed to move reputation, check the logs.");
            });
        } else {
            this.appService.toastError("Can't move this reputation any more top.");
        }
    }

    public moveDown(reputation: ReputationWithCount) {
        let nextReputation = this.reputations.find(r => r.type == reputation.type && r.orderSequence == reputation.orderSequence + 1);
        if (nextReputation) {
            nextReputation.orderSequence--;
            reputation.orderSequence++;

            this.loading = true;
            this.reputationService.saveReputation(reputation.id, this.getReputationSaveRequest(reputation)).subscribe(() => {
                this.reputationService.saveReputation(nextReputation.id, this.getReputationSaveRequest(nextReputation)).subscribe(() => {
                    this.getReputations();
                    this.reputationService.fetchReputations(ReputationType.NEGATIVE);
                }, error => {
                    console.error("Failed to move reputation", error);
                    this.appService.toastError("Failed to move reputation, check the logs.");
                });
            }, error => {
                console.error("Failed to move reputation", error);
                this.appService.toastError("Failed to move reputation, check the logs.");
            });
        } else {
            this.appService.toastError("Can't move this reputation any more bottom.");
        }
    }

    private getReputationSaveRequest(reputation: ReputationWithCount) {
        return <SaveReputationRequest>{
            name: reputation.name,
            reputationType: reputation.type,
            commentRequired: reputation.commentRequired,
            orderSequence: reputation.orderSequence
        }
    }

    private getReputations() {
        this.loading = true;
        this.reputationService.getReputationsWithCount().subscribe(reputations => {
            this.reputations = reputations;
            let negativeReputations = reputations.filter(r => r.type == ReputationType.NEGATIVE).sort((a, b) => a.orderSequence - b.orderSequence);
            let positiveReputations = reputations.filter(r => r.type == ReputationType.POSITIVE).sort((a, b) => a.orderSequence - b.orderSequence);
            this.negativeReputationsCount = negativeReputations.length;
            this.positiveReputationsCount = positiveReputations.length;
            this.negativeReputationsDataSource = new ReputationsDataSource(negativeReputations);
            this.positiveReputationsDataSource = new ReputationsDataSource(positiveReputations);
            this.loading = false;
        });
    }
}

export class ReputationsDataSource extends DataSource<ReputationWithCount> {
    public reputations: BehaviorSubject<ReputationWithCount[]>;

    constructor(reputations: ReputationWithCount[]) {
        super();
        this.reputations = new BehaviorSubject<ReputationWithCount[]>(reputations);
    }

    public connect(collectionViewer: CollectionViewer) {
        return this.reputations.asObservable();
    }

    public disconnect(collectionViewer: CollectionViewer) {
    }
}