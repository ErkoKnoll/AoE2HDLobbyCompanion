import { Component, Input, Output, EventEmitter } from '@angular/core';
import { shell } from 'electron';

import { TrackingService } from '../../shared';

@Component({
    selector: 'help-page',
    templateUrl: 'help-page.component.html'
})
export class HelpPageComponent {
    @Output() proceed = new EventEmitter<boolean>();
    @Input() showControls;
    public skipNextTime = false;

    constructor(private trackingService: TrackingService) {
    }

    public openNethook() {
        shell.openExternal("https://github.com/SteamRE/SteamKit/tree/master/Resources/NetHook2");
        this.trackingService.sendEvent("HelpPage", "NetHookLinkOpened");
    }

    public proceedSubmit() {
        this.proceed.emit(this.skipNextTime);
    }
}
