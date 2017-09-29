import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { MdButtonModule, MdDialogModule, MdTableModule, MdTabsModule, MdTooltipModule } from '@angular/material';
import { CdkTableModule } from '@angular/cdk';

import { CenterSpinnerModule, ConfirmationDialogModule } from '../'
import { MatchDetailsDialogComponent } from './match-details-dialog.component';

@NgModule({
    imports: [BrowserModule, MdButtonModule, MdDialogModule, MdTableModule, MdTabsModule, MdTooltipModule, CdkTableModule, CenterSpinnerModule, ConfirmationDialogModule],
    exports: [MatchDetailsDialogComponent],
    declarations: [MatchDetailsDialogComponent],
    entryComponents: [MatchDetailsDialogComponent]
})
export class MatchDetailsDialogModule { }