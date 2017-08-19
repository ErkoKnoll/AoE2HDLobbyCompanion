import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { MdButtonModule, MdTableModule, MdDialogModule, MdCheckboxModule, MdTooltipModule } from '@angular/material';
import { FormsModule } from '@angular/forms';
import { CdkTableModule } from '@angular/cdk';

import { LobbyService } from './shared';
import { CenterSpinnerModule, ConfirmationDialogModule, AssignReputationDialogModule, UserProfileDialogModule } from '../shared'
import { LobbyPageComponent } from './lobby-page';
import { InitialPageComponent } from './initial-page';
import { RunningSessionPageComponent } from './running-session-page';
import { SessionHelpPageComponent } from './session-help-page';
import { MoreOptionsDialogComponent } from './more-options-dialog';

@NgModule({
    imports: [BrowserModule, MdButtonModule, MdTableModule, CdkTableModule, MdDialogModule, MdCheckboxModule, CenterSpinnerModule, ConfirmationDialogModule, AssignReputationDialogModule, UserProfileDialogModule, FormsModule, MdTooltipModule],
    exports: [LobbyPageComponent],
    declarations: [LobbyPageComponent, InitialPageComponent, RunningSessionPageComponent, SessionHelpPageComponent, MoreOptionsDialogComponent],
    providers: [LobbyService],
    entryComponents: [MoreOptionsDialogComponent]
})
export class LobbyModule { }