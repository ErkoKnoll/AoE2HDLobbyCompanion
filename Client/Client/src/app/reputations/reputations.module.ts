import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { MdButtonModule, MdInputModule, MdTableModule, MdTooltipModule, MdCheckboxModule } from '@angular/material';
import { CdkTableModule } from '@angular/cdk';

import { CenterSpinnerModule } from '../shared'
import { ReputationsPageComponent } from './reputations-page';

@NgModule({
    imports: [BrowserModule, FormsModule, MdButtonModule, MdInputModule, MdCheckboxModule, MdTableModule, MdTooltipModule, CdkTableModule, CenterSpinnerModule],
    exports: [ReputationsPageComponent],
    declarations: [ReputationsPageComponent],
    entryComponents: []
})
export class ReputationsModule { }