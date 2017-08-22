import { Component, OnInit } from '@angular/core';
import { DataSource, CollectionViewer } from '@angular/cdk';
import { MdDialog } from '@angular/material';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import { ReputationService, ReputationWithCount, ReputationType } from '../../shared';
import { SaveReputationDialogComponent, SaveReputationDialogData } from '../save-reputation-dialog';

@Component({
    selector: 'reputations-page',
    templateUrl: 'reputations-page.component.html',
    styleUrls: ['./reputations-page.component.scss']
})
export class ReputationsPageComponent implements OnInit {
    public loading = true;
    public negativeReputationsDataSource: ReputationsDataSource;
    public positiveReputationsDataSource: ReputationsDataSource;
    public displayedColumns = ["name", "commentRequired", "total", "actions"];
    private reputations: ReputationWithCount[];

    constructor(private reputationService: ReputationService, private dialog: MdDialog) {
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

    private getReputations() {
        this.loading = true;
        this.reputationService.getReputationsWithCount().subscribe(reputations => {
            this.reputations = reputations;
            this.negativeReputationsDataSource = new ReputationsDataSource(reputations.filter(r => r.type == ReputationType.NEGATIVE).sort((a, b) => a.orderSequence - b.orderSequence));
            this.positiveReputationsDataSource = new ReputationsDataSource(reputations.filter(r => r.type == ReputationType.POSITIVE).sort((a, b) => a.orderSequence - b.orderSequence));
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