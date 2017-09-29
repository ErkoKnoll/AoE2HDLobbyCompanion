import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { MdButtonModule, MdTableModule, MdDialogModule, MdPaginatorModule } from '@angular/material';
import { CdkTableModule } from '@angular/cdk';

import { UserProfileDialogModule } from "../user-profile-dialog";
import { CenterSpinnerModule, ConfirmationDialogModule } from '../';
import { ReputationDetailsDialogComponent } from './reputation-details-dialog.component';

@NgModule({
    imports: [BrowserModule, FormsModule, MdButtonModule, MdTableModule, MdDialogModule, MdPaginatorModule, CdkTableModule, CenterSpinnerModule, ConfirmationDialogModule, UserProfileDialogModule],
    exports: [ReputationDetailsDialogComponent],
    declarations: [ReputationDetailsDialogComponent],
    entryComponents: [ReputationDetailsDialogComponent]
})
export class ReputationDetailsDialogModule { }