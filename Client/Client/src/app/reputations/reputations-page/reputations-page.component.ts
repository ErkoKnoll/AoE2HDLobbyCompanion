import { Component, OnInit } from '@angular/core';
import { DataSource, CollectionViewer } from '@angular/cdk';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import { ReputationService, ReputationWithCount, ReputationType } from '../../shared';

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

    constructor(private reputationService: ReputationService) {
    }

    public ngOnInit() {
        this.getReputations();
    }

    private getReputations() {
        this.loading = true;
        this.reputationService.getReputationsWithCount().subscribe(reputations => {
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