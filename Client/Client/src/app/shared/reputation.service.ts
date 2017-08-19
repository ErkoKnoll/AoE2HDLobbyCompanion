import { Injectable } from '@angular/core';

import { HttpService } from './';
import { Reputation, ReputationType } from '../app.models';

@Injectable()
export class ReputationService {
    private reputations: Reputation[] = [];

    constructor(private httpService: HttpService) {
    }

    public getNegativeReputations() {
        return this.getReputations(ReputationType.NEGATIVE);
    }

    public getPositiveReputations() {
        return this.getReputations(ReputationType.POSITIVE);
    }

    public getReputations(reputationType: ReputationType) {
        if (this.reputations.length == 0) {
            return this.fetchReputations(reputationType);
        } else {
            return new Promise<Reputation[]>((resolve, reject) => resolve(this.getReputationsFromCache(reputationType)));
        }
    }

    public fetchReputations(reputationType) {
        return new Promise<Reputation[]>((resolve, reject) => {
            this.httpService.get<Reputation[]>("/api/reputations").subscribe(reputations => {
                this.reputations = reputations;
                resolve(this.getReputationsFromCache(reputationType));
            }, error => {
                reject(error);
            });
        });
    }

    public assignReputation(request: AssignReputationRequest) {
        return this.httpService.put("/api/userReputations", request);
    }

    public deleteReputation(id: number) {
        return this.httpService.delete("/api/userReputations/" + id);
    }

    private getReputationsFromCache(reputationType: ReputationType) {
        return this.reputations.filter(r => r.type == reputationType).sort((a, b) => a.orderSequence - b.orderSequence);
    }

}

export interface AssignReputationRequest {
    playerSteamId: string;
    lobbyId: string;
    lobbySlotId: number;
    reputationId: number;
    comment: string;
}