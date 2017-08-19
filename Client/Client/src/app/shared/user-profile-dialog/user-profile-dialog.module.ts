import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { MdButtonModule, MdDialogModule, MdTableModule, MdTooltipModule } from '@angular/material';
import { CdkTableModule } from '@angular/cdk';

import { CenterSpinnerModule, ConfirmationDialogModule } from '../'
import { UserProfileDialogComponent } from './user-profile-dialog.component';

@NgModule({
    imports: [BrowserModule, MdButtonModule, MdDialogModule, MdTableModule, MdTooltipModule, CdkTableModule, CenterSpinnerModule, ConfirmationDialogModule],
    exports: [UserProfileDialogComponent],
    declarations: [UserProfileDialogComponent],
    entryComponents: [UserProfileDialogComponent]
})
export class UserProfileDialogModule { }