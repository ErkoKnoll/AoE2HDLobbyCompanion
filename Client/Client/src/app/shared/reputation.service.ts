import { Injectable } from '@angular/core';

import { HttpService } from './';
import { Reputation, ReputationType, User, Lobby, UserReputation } from '../app.models';

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

    public getReputationsWithCount() {
        return this.httpService.get<ReputationWithCount[]>("/api/manageReputations");
    }

    public fetchReputations(reputationType: ReputationType) {
        return new Promise<Reputation[]>((resolve, reject) => {
            this.httpService.get<Reputation[]>("/api/reputations").subscribe(reputations => {
                this.reputations = reputations;
                resolve(this.getReputationsFromCache(reputationType));
            }, error => {
                reject(error);
            });
        });
    }

    public getUserReputationsForReputationType(id: number) {
        return this.httpService.get<UserReputation[]>("/api/manageReputations/" + id);
    }

    public assignReputation(request: AssignReputationRequest) {
        return this.httpService.put("/api/userReputations", request);
    }

    public deleteReputation(id: number) {
        return this.httpService.delete("/api/userReputations/" + id);
    }

    public addReputation(request: SaveReputationRequest) {
        return this.httpService.put("/api/manageReputations", request);
    }

    public saveReputation(id: number, request: SaveReputationRequest) {
        return this.httpService.post("/api/manageReputations/" + id, request);
    }

    private getReputationsFromCache(reputationType: ReputationType) {
        return this.reputations.filter(r => r.type == reputationType).sort((a, b) => a.orderSequence - b.orderSequence);
    }

    public deleteReputationType(id: number, migrateTo: number) {
        return this.httpService.delete("/api/manageReputations/" + id + "/?migrateTo=" + migrateTo);
    }
}

export interface AssignReputationRequest {
    playerSteamId: string;
    lobbyId: string;
    lobbySlotId: number;
    reputationId: number;
    comment: string;
}

export interface ReputationWithCount extends Reputation {
    total: number;
}

export interface SaveReputationRequest {
    name: string;
    reputationType: number;
    commentRequired: boolean;
    orderSequence?: number;
}