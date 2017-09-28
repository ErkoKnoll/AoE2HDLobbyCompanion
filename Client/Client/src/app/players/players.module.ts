import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { MdButtonModule, MdInputModule, MdTableModule, MdCheckboxModule, MdDialogModule, MdPaginatorModule, MdSortModule } from '@angular/material';
import { CdkTableModule } from '@angular/cdk';

import { CenterSpinnerModule } from '../shared'
import { PlayersPageComponent } from './players-page';

@NgModule({
    imports: [BrowserModule, FormsModule, MdButtonModule, MdInputModule, MdCheckboxModule, MdTableModule, MdDialogModule, MdPaginatorModule, MdSortModule, CdkTableModule, CenterSpinnerModule],
    exports: [PlayersPageComponent],
    declarations: [PlayersPageComponent],
    entryComponents: []
})
export class PlayersModule { }