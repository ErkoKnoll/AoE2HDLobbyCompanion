import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { MdPaginator, MdSort, MdDialog } from '@angular/material';
import { DataSource } from '@angular/cdk';

import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/startWith';
import 'rxjs/add/observable/merge';
import 'rxjs/add/observable/fromEvent';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/map';

import { AppService, HttpService, TrackingService, UserProfileDialogComponent, UserProfileDialogData } from '../../shared';
import { Player } from '../../app.models';

@Component({
    selector: 'players-page',
    templateUrl: 'players-page.component.html',
    styleUrls: ['./players-page.component.scss']
})
export class PlayersPageComponent implements OnInit {
    public loading = true;
    public dm = false;
    public playersDataSource: PlayersDataSource;
    public displayedColumns = ["name", "rank", "location", "games", "negativeRep", "positiveRep", "totalGames", "winRatio", "dropRatio"];
    private players: Player[];
    @ViewChild(MdPaginator) paginator: MdPaginator;
    @ViewChild(MdSort) sort: MdSort;
    @ViewChild("nameFilter") nameFilter: ElementRef;
    @ViewChild("locationFilter") locationFilter: ElementRef;

    constructor(private appService: AppService, private httpService: HttpService, private trackingService: TrackingService, private dialogController: MdDialog) {
    }

    public ngOnInit() {
        this.sort.direction = "desc";
        this.sort.active = "games";

        Observable.fromEvent(this.nameFilter.nativeElement, 'keyup').debounceTime(500).distinctUntilChanged().subscribe(() => {
            if (this.playersDataSource) {
                this.playersDataSource.nameFilter = this.nameFilter.nativeElement.value;
            }
        });
        Observable.fromEvent(this.locationFilter.nativeElement, 'keyup').debounceTime(500).distinctUntilChanged().subscribe(() => {
            if (this.playersDataSource) {
                this.playersDataSource.locationFilter = this.locationFilter.nativeElement.value;
            }
        });

        this.getPlayers();
    }

    public dmChanged() {
        this.playersDataSource.dm = this.dm;
    }

    public openUserProfileDialog(player: Player) {
        let dialog = this.dialogController.open(UserProfileDialogComponent, {
            data: <UserProfileDialogData>{
                steamId: player.sSteamId,
                reputationDeleted: () => this.getPlayers()
            },
            width: window.innerWidth * 0.75 + "px",
        });
        this.trackingService.sendEvent("PlayersPage", "OpenUserProfile");
    }

    private getPlayers() {
        this.loading = true;
        this.httpService.get<Player[]>("/api/userProfile").subscribe(players => {
            this.players = players;
            this.playersDataSource = new PlayersDataSource(players, this.paginator, this.sort);
            this.loading = false;
        }, error => {
            console.error("Failed to fetch players", error);
            this.appService.toastError("Failed to retrieve players.");
        });
    }
}

export class PlayersDataSource extends DataSource<Player> {
    public players: BehaviorSubject<Player[]>;
    private dmChange = new BehaviorSubject(false);
    private nameFilterChange = new BehaviorSubject("");
    private locationFilterChange = new BehaviorSubject("");

    constructor(matches: Player[], private paginator: MdPaginator, private sort: MdSort) {
        super();
        this.players = new BehaviorSubject<Player[]>(matches);
    }

    public get dm() { return this.dmChange.value; }
    public set dm(value: boolean) { this.dmChange.next(value); }

    public get nameFilter() { return this.nameFilterChange.value; }
    public set nameFilter(value: string) { this.nameFilterChange.next(value); }

    public get locationFilter() { return this.locationFilterChange.value; }
    public set locationFilter(value: string) { this.locationFilterChange.next(value); }

    public connect() {
        let displayDataChanges = [
            this.players,
            this.paginator.page,
            this.sort.mdSortChange,
            this.dmChange,
            this.nameFilterChange,
            this.locationFilterChange
        ];

        return Observable.merge(...displayDataChanges).map(() => {
            let data = this.getSortedData();
            let startIndex = this.paginator.pageIndex * this.paginator.pageSize;
            return data.splice(startIndex, this.paginator.pageSize);
        });
    }

    public disconnect() {
    }

    private getSortedData() {
        let data = this.players.value.slice();
        if (this.nameFilter.length > 0) {
            data = data.filter(p => p.name.toLowerCase().indexOf(this.nameFilter.toLowerCase()) != -1);
        }
        if (this.locationFilter.length > 0) {
            data = data.filter(p => p.profile && p.profile.location && p.profile.location.toLowerCase() == this.locationFilter.toLowerCase());
        }
        data.forEach(player => {
            if (player.profile.profilePrivate && !player.profileDataFetched) {
                if (this.sort.direction == "asc") {
                    player.gameStats.totalGamesRM = 999999;
                    player.gameStats.totalGamesDM = 999999;
                    player.gameStats.winRatioRM = 999;
                    player.gameStats.winRatioDM = 999;
                    player.gameStats.dropRatioRM = 999;
                    player.gameStats.dropRatioDM = 999;
                } else {
                    player.gameStats.totalGamesRM = -1;
                    player.gameStats.totalGamesDM = -1;
                    player.gameStats.winRatioRM = -1;
                    player.gameStats.winRatioDM = -1;
                    player.gameStats.dropRatioRM = -1;
                    player.gameStats.dropRatioDM = -1;
                }
            }
        })
        if (!this.sort.active || this.sort.direction == '') { return data; }

        return data.sort((a, b) => {
            let propertyA: number | string = '';
            let propertyB: number | string = '';

            try {
                switch (this.sort.active) {
                    case "name":
                        let nameA = "", nameB = "";
                        if (a.name) nameA = a.name;
                        if (b.name) nameB = b.name;
                        return this.sort.direction == "asc" ? nameA.localeCompare(nameB) : nameA.localeCompare(nameB) * -1;
                    case "location":
                        let locationA = "", locationB = "";
                        if (a.profile.location) locationA = a.profile.location;
                        if (b.profile.location) locationB = b.profile.location;
                        return this.sort.direction == "asc" ? locationA.localeCompare(locationB) : locationA.localeCompare(locationB) * -1;
                    case 'rank': [propertyA, propertyB] = [!this.dm ? a.rankRM : a.rankDM, !this.dm ? b.rankRM : b.rankDM]; break;
                    case 'games': [propertyA, propertyB] = [a.reputationStats.games, b.reputationStats.games]; break;
                    case 'negativeRep': [propertyA, propertyB] = [a.reputationStats.negativeReputation, b.reputationStats.negativeReputation]; break;
                    case 'positiveRep': [propertyA, propertyB] = [a.reputationStats.positiveReputation, b.reputationStats.positiveReputation]; break;
                    case 'totalGames': [propertyA, propertyB] = [!this.dm ? a.gameStats.totalGamesRM : a.gameStats.totalGamesDM, !this.dm ? b.gameStats.totalGamesRM : b.gameStats.totalGamesDM]; break;
                    case 'winRatio': [propertyA, propertyB] = [!this.dm ? a.gameStats.winRatioRM : a.gameStats.winRatioDM, !this.dm ? b.gameStats.winRatioRM : b.gameStats.winRatioDM]; break;
                    case 'dropRatio': [propertyA, propertyB] = [!this.dm ? a.gameStats.dropRatioRM : a.gameStats.dropRatioDM, !this.dm ? b.gameStats.dropRatioRM : b.gameStats.dropRatioDM]; break;
                }
            } catch (e) {
                console.error(e)
            }

            let valueA = isNaN(+propertyA) ? propertyA : +propertyA;
            let valueB = isNaN(+propertyB) ? propertyB : +propertyB;

            return (valueA < valueB ? -1 : 1) * (this.sort.direction == "asc" ? 1 : -1);
        });
    }
}