import { Injectable } from '@angular/core';

import { ConfigurationService } from './';

@Injectable()
export class TrackingService {
    private analytics;
    private version: "1.2.0";

    constructor(private configurationService: ConfigurationService) {
        try {
            let Analytics = require('electron-google-analytics');
            this.analytics = new Analytics.default("UA-103852230-1", {
                version: this.version
            });
        } catch (e) {
            console.error("Failed to init stats service", e);
        }
    }

    public sendEvent(category: string, action: string) {
        try {
            this.analytics.event(category, action, {
                clientID: this.getClientId()
            });
        } catch (e) {
            console.error("Failed to send event statistics", e);
        }
    }

    public logException(exception: string) {
        try {
            this.analytics.exception(exception, 0, this.getClientId());
        } catch (e) {
            console.error("Failed to send error statistics", e);
        }
    }

    private getClientId() {
        let clientId: string = null;
        if (this.configurationService.configuration) {
            clientId = this.configurationService.configuration.clientId;
        }
        return clientId;
    }
}