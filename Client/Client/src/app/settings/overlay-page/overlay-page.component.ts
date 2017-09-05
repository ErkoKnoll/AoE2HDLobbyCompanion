import { Component, OnInit } from '@angular/core';

import { Configuration } from '../../app.models';
import { ConfigurationService, AppService, TrackingService } from '../../shared';

@Component({
    selector: 'overlay-page',
    templateUrl: 'overlay-page.component.html'
})
export class OverlayPageComponent implements OnInit {
    public loading = false;
    public conf: Configuration;
    public overlayActiveTopPosition: number;
    public overlayActiveLeftPosition: number;
    public overlayInactiveTopPosition: number;
    public overlayInactiveLeftPosition: number;
    public showOverlayPositioningHelp = false;

    constructor(private configurationService: ConfigurationService, private appService: AppService, private trackingService: TrackingService) {
    }

    public ngOnInit() {
        this.readConfiguration();
    }

    private readConfiguration() {
        this.loading = true;
        this.configurationService.getConfiguration(() => {
            this.conf = this.configurationService.configuration;
            this.overlayActiveTopPosition = this.conf.overlayActiveTopPosition;
            this.overlayActiveLeftPosition = this.conf.overlayActiveLeftPosition;
            this.overlayInactiveTopPosition = this.conf.overlayInactiveTopPosition;
            this.overlayInactiveLeftPosition = this.conf.overlayInactiveLeftPosition;
            this.loading = false;
        });
    }

    public toggleOverlayPositioningHelp() {
        this.showOverlayPositioningHelp = !this.showOverlayPositioningHelp;
    }

    public save() {
        if (!(this.overlayActiveTopPosition + "").match(/^[+\-]?\d+$/) || !(this.overlayActiveLeftPosition + "").match(/^[+\-]?\d+$/) || !(this.overlayInactiveTopPosition + "").match(/^[+\-]?\d+$/) || !(this.overlayInactiveLeftPosition + "").match(/^[+\-]?\d+$/)) {
            this.appService.toastError("Overlay position coordinates have to be integers.");
            return;
        }
        this.loading = true;
        this.configurationService.configuration = this.conf;
        this.configurationService.configuration.overlayActiveTopPosition = this.overlayActiveTopPosition;
        this.configurationService.configuration.overlayActiveLeftPosition = this.overlayActiveLeftPosition;
        this.configurationService.configuration.overlayInactiveTopPosition = this.overlayInactiveTopPosition;
        this.configurationService.configuration.overlayInactiveLeftPosition = this.overlayInactiveLeftPosition;
        this.configurationService.saveConfiguration().subscribe(() => {
            this.appService.toastSuccess("Settings saved.");
            this.readConfiguration();
            this.trackingService.sendEvent("Settings", "OverlaySettingsSaved");
        }, error => {
            this.loading = false;
            console.error("Failed to save settings", error);
            this.appService.toastError("Failed to save settings, check the logs.");
        });
    }
}