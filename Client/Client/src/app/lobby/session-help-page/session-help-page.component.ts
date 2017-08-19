import { Component, Output, EventEmitter } from '@angular/core';
import { shell } from 'electron';

import { TrackingService } from '../../shared';

@Component({
    selector: 'session-help-page',
    templateUrl: 'session-help-page.component.html'
})
export class SessionHelpPageComponent {
    @Output() proceed = new EventEmitter<boolean>();
    public skipNextTime = false;

    constructor(private trackingService: TrackingService) {
    }

    public openNethook() {
        shell.openExternal("https://github.com/SteamRE/SteamKit/tree/master/Resources/NetHook2");
        this.trackingService.sendEvent("Lobby", "NetHookLinkOpened");
    }

    public proceedSubmit() {
        this.proceed.emit(this.skipNextTime);
    }
}
