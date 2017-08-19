import { Component, Input } from '@angular/core';
import { MdButtonModule } from '@angular/material';
import { RouterModule } from '@angular/router';
import { remote, shell } from 'electron';

import { TrackingService } from '../';

@Component({
    selector: 'navbar',
    templateUrl: './navbar.component.html',
    styleUrls: ['./navbar.component.scss']
})
export class NavBarComponent {
    @Input() appState: number;

    constructor(private trackingService: TrackingService) {
    }

    public toggleLogs() {
        remote.getCurrentWindow().webContents.toggleDevTools();
    }

    public openGithub() {
        shell.openExternal("https://github.com/ThorConzales/AoE2HDLobbyCompanion");
        this.trackingService.sendEvent("NavBar", "OpenGithub");
    }

    public openReddit() {
        shell.openExternal("https://www.reddit.com/r/aoe2lc");
        this.trackingService.sendEvent("NavBar", "OpenReddit");
    }
}
