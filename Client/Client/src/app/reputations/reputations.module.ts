import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { MdButtonModule, MdInputModule, MdTableModule, MdTooltipModule, MdCheckboxModule, MdDialogModule, MdSelectModule, MdPaginatorModule } from '@angular/material';
import { CdkTableModule } from '@angular/cdk';

import { CenterSpinnerModule, ConfirmationDialogModule, UserProfileDialogModule } from '../shared'
import { ReputationsPageComponent } from './reputations-page';
import { SaveReputationDialogComponent } from './save-reputation-dialog';
import { DeleteReputationDialogComponent } from './delete-reputation-dialog';
import { ReputationDetailsDialogComponent } from './reputation-details-dialog';

@NgModule({
    imports: [BrowserModule, FormsModule, MdButtonModule, MdInputModule, MdCheckboxModule, MdTableModule, MdTooltipModule, MdDialogModule, MdSelectModule, MdPaginatorModule, CdkTableModule, CenterSpinnerModule, ConfirmationDialogModule, UserProfileDialogModule],
    exports: [ReputationsPageComponent],
    declarations: [ReputationsPageComponent, SaveReputationDialogComponent, DeleteReputationDialogComponent, ReputationDetailsDialogComponent],
    entryComponents: [SaveReputationDialogComponent, DeleteReputationDialogComponent, ReputationDetailsDialogComponent]
})
export class ReputationsModule { }