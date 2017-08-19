import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { MdButtonModule, MdInputModule, MdSlideToggleModule } from '@angular/material';

import { CenterSpinnerModule } from '../shared'
import { SettingsPageComponent } from './settings-page';
import { GeneralPageComponent } from './general-page';

@NgModule({
    imports: [BrowserModule, FormsModule, MdButtonModule, MdInputModule, MdSlideToggleModule, CenterSpinnerModule],
    exports: [SettingsPageComponent],
    declarations: [SettingsPageComponent, GeneralPageComponent],
    entryComponents: []
})
export class SettingsModule { }