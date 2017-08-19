import { Component, Inject } from '@angular/core';
import { MD_DIALOG_DATA } from '@angular/material';
import { shell } from 'electron';

import { Version } from '../../app.models';
import { TrackingService } from '../';

@Component({
    selector: 'update-available-dialog',
    templateUrl: './update-available-dialog.component.html',
})
export class UpdateAvailableComponent {

    constructor( @Inject(MD_DIALOG_DATA) private version: Version, private trackingService: TrackingService) {
    }

    public update() {
        shell.openExternal(this.version.downloadUrl);
        this.trackingService.sendEvent("UpdateAvailableDialog", "UpdateDownloaded");
    }

    public skip() {
        this.trackingService.sendEvent("UpdateAvailableDialog", "UpdateSkipped");
    }
}