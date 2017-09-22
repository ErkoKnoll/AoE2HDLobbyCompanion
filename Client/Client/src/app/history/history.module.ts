import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { MdButtonModule, MdInputModule, MdTableModule, MdCheckboxModule, MdDialogModule, MdPaginatorModule } from '@angular/material';
import { CdkTableModule } from '@angular/cdk';

import { CenterSpinnerModule } from '../shared'
import { HistoryPageComponent } from './history-page';

@NgModule({
    imports: [BrowserModule, FormsModule, MdButtonModule, MdInputModule, MdCheckboxModule, MdTableModule, MdDialogModule, MdPaginatorModule, CdkTableModule, CenterSpinnerModule],
    exports: [HistoryPageComponent],
    declarations: [HistoryPageComponent],
    entryComponents: []
})
export class HistoryModule { }