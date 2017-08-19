import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { MdButtonModule, MdDialogModule, MdInputModule, MdSelectModule } from '@angular/material';

import { CenterSpinnerModule } from '../'
import { AssignReputationDialogComponent } from './assign-reputation-dialog.component';

@NgModule({
    imports: [BrowserModule, FormsModule, MdButtonModule, MdDialogModule, MdInputModule, MdSelectModule, CenterSpinnerModule],
    exports: [AssignReputationDialogComponent],
    declarations: [AssignReputationDialogComponent],
    entryComponents: [AssignReputationDialogComponent]
})
export class AssignReputationDialogModule { }