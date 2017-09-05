import { Component } from '@angular/core';
import { MdDialogRef } from '@angular/material';

import { AppService, CommandService, TrackingService, Commands } from '../../shared';

@Component({
    selector: 'more-options-dialog',
    templateUrl: './more-options-dialog.component.html',
})
export class MoreOptionsDialogComponent {
    public loading = false;

    constructor(private appService: AppService, private commandService: CommandService, private trackingService: TrackingService, private dialog: MdDialogRef<MoreOptionsDialogComponent>) {
    }

    public calculateBalancedTeamsBasedOnRank() {
        this.calculateBalancedTeams(Commands.OUT.CALCULATE_BALANCED_TEAMS_BASED_ON_RANK);
    }

    public calculateBalancedTeamsBasedOnTotalGames() {
        this.calculateBalancedTeams(Commands.OUT.CALCULATE_BALANCED_TEAMS_BASED_ON_TOTAL_GAMES);
    }

    public calculateBalancedTeamsBasedOnWinRatio() {
        this.calculateBalancedTeams(Commands.OUT.CALCULATE_BALANCED_TEAMS_BASED_ON_WIN_RATIO);
    }

    public copyPlayerStats() {
        this.loading = true;
        this.commandService.sendCommandAndReadRawResponse(Commands.OUT.COPY_PLAYER_STATS).subscribe(response => {
            if (response.text()) {
                this.appService.toastError(response.text());
                this.loading = false;
            } else {
                this.appService.toastSuccess("Player stats were copied to your clipboard, you can paste them to lobby chat.");
                this.dialog.close();
            }
        }, error => {
            this.loading = false;
            console.log("Failed to copy player stats", error);
            this.appService.toastError("Failed to copy player stats.");
        });
    }

    private calculateBalancedTeams(command: Commands.OUT) {
        this.loading = true;
        this.commandService.sendCommandAndReadRawResponse(command).subscribe(response => {
            if (response.text()) {
                this.appService.toastError(response.text());
                this.loading = false;
            } else {
                this.appService.toastSuccess("Balanced teams were copied to your clipboard, you can paste them to lobby chat.");
                this.dialog.close();
            }
        }, error => {
            this.loading = false;
            console.log("Failed to calculate balanced teams", error);
            this.appService.toastError("Failed to calculate balanced teams.");
        });
    }
}