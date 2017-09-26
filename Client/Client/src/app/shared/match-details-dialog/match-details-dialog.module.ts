import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { MdButtonModule, MdDialogModule, MdTableModule, MdTabsModule } from '@angular/material';
import { CdkTableModule } from '@angular/cdk';

import { CenterSpinnerModule } from '../'
import { MatchDetailsDialogComponent } from './match-details-dialog.component';

@NgModule({
    imports: [BrowserModule, MdButtonModule, MdDialogModule, MdTableModule, MdTabsModule, CdkTableModule, CenterSpinnerModule],
    exports: [MatchDetailsDialogComponent],
    declarations: [MatchDetailsDialogComponent],
    entryComponents: [MatchDetailsDialogComponent]
})
export class MatchDetailsDialogModule { }