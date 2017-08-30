import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { MdButtonModule, MdInputModule, MdTableModule, MdTooltipModule, MdCheckboxModule, MdDialogModule, MdSelectModule } from '@angular/material';
import { CdkTableModule } from '@angular/cdk';

import { CenterSpinnerModule } from '../shared'
import { ReputationsPageComponent } from './reputations-page';
import { SaveReputationDialogComponent } from './save-reputation-dialog';
import { DeleteReputationDialogComponent } from './delete-reputation-dialog';

@NgModule({
    imports: [BrowserModule, FormsModule, MdButtonModule, MdInputModule, MdCheckboxModule, MdTableModule, MdTooltipModule, MdDialogModule, MdSelectModule, CdkTableModule, CenterSpinnerModule],
    exports: [ReputationsPageComponent],
    declarations: [ReputationsPageComponent, SaveReputationDialogComponent, DeleteReputationDialogComponent],
    entryComponents: [SaveReputationDialogComponent, DeleteReputationDialogComponent]
})
export class ReputationsModule { }