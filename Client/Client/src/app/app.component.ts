import { Component, ViewContainerRef, NgZone } from '@angular/core';
import { shell, remote } from 'electron';

import { ToastsManager } from 'ng2-toastr/ng2-toastr';

import { CommandService, ConfigurationService, AppService, TrackingService, Commands } from './shared';
import { Configuration } from './app.models';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss']
})
export class AppComponent {
    public version: string;
    public loadingStage = "Initializing...";
    public appState = AppState.INITIALIZING;
    public apiKey = "";

    constructor(private commandService: CommandService, private configurationService: ConfigurationService, private appService: AppService, private trackingService: TrackingService, toastsManager: ToastsManager, viewContainerRef: ViewContainerRef) {
        toastsManager.setRootViewContainerRef(viewContainerRef);
        this.commandService.startListeningIncomingCommands(() => this.initiateSetUp());
        this.version = this.appService.stringVersion;
    }

    public acceptTos() {
        this.appState = AppState.INSERT_API_KEY;
        this.trackingService.sendEvent("Initial", "TOSAccepted");
    }

    public declineTos() {
        remote.getCurrentWindow().close();
        this.trackingService.sendEvent("Initial", "TOSDeclined");
    }

    public insertApiKeyProceed() {
        this.appState = AppState.SETTING_UP;
        this.loadingStage = "Setting up...";
        this.configurationService.configuration.tosAccepted = true;
        this.configurationService.configuration.steamApiKey = this.apiKey;
        this.configurationService.saveConfiguration().subscribe(() => this.appState = AppState.RUNNING, error => {
            console.error("Failed to save configuration", error);
            this.appService.toastError("Failed to save configuration");
            this.appState = AppState.RUNNING
        });
        if (this.apiKey.length > 0) {
            this.trackingService.sendEvent("Initial", "ApiKeyAdded");
        } else {
            this.trackingService.sendEvent("Initial", "ApiKeySkipped");
        }
    }

    public openNethook() {
        shell.openExternal("https://github.com/SteamRE/SteamKit/tree/master/Resources/NetHook2");
        this.trackingService.sendEvent("Initial", "NetHookLinkOpened");
    }

    public openApiKey() {
        shell.openExternal("https://steamcommunity.com/dev/apikey");
        this.trackingService.sendEvent("Initial", "ApiLinkOpened");
    }

    public openVac() {
        shell.openExternal("http://store.steampowered.com/search/?category2=8");
        this.trackingService.sendEvent("Initial", "VACLinkOpened");
    }

    private readConfiguration() {
        this.loadingStage = "Reading configuration...";
        this.appState = AppState.READING_CONFIGURATION;
        this.configurationService.getConfiguration(success => {
            if (this.configurationService.configuration.tosAccepted) {
                this.appState = AppState.RUNNING;
            } else {
                this.appState = AppState.TOS;
            }
            if (!success) {
                this.appService.toastError("Failed to read configuration.");
            }
        });
    }

    private initiateSetUp() {
        this.loadingStage = "Setting up...";
        this.appState = AppState.SETTING_UP;
        this.commandService.sendCommandAndReadRawResponse<string>(Commands.OUT.SET_UP).subscribe(errorMessage => {
            if (errorMessage && errorMessage.text() && errorMessage.text().length > 0) {
                this.appService.toastError(errorMessage.text());
                console.error(errorMessage.text());
            } else {
                this.readConfiguration();
            }
        });
    }
}

enum AppState {
    INITIALIZING = 0,
    SETTING_UP = 1,
    READING_CONFIGURATION = 2,
    RUNNING = 3,
    TOS = 4,
    INSERT_API_KEY = 5,
}
