import 'zone.js';
import 'reflect-metadata';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { MdButtonModule, MdInputModule, MdDialogModule } from '@angular/material';

import { ToastModule } from 'ng2-toastr/ng2-toastr';

import { AppRoutingModule } from './app-routing.module';
import { AppService, HttpService, CommandService, ConfigurationService, ReputationService, TrackingService, UpdateAvailableModule, MatchDetailsDialogModule } from './shared';
import { AppComponent } from './app.component';
import { LobbyModule } from './lobby';
import { SettingsModule } from './settings';
import { ReputationsModule } from './reputations';
import { HistoryModule } from './history';
import { PlayersModule } from './players';
import { CenterSpinnerModule, NavBarModule } from './shared'

@NgModule({
    declarations: [
        AppComponent
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        FormsModule,
        HttpModule,
        MdButtonModule,
        MdInputModule,
        MdDialogModule,
        ToastModule.forRoot(),
        AppRoutingModule,
        CenterSpinnerModule,
        NavBarModule,
        LobbyModule,
        SettingsModule,
        ReputationsModule,
        HistoryModule,
        PlayersModule,
        UpdateAvailableModule,
        MatchDetailsDialogModule
    ],
    providers: [AppService, HttpService, CommandService, ConfigurationService, ReputationService, TrackingService],
    bootstrap: [AppComponent]
})
export class AppModule { }
