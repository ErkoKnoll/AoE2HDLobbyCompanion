import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { MdButtonModule, MdInputModule, MdSlideToggleModule, MdTabsModule } from '@angular/material';

import { CenterSpinnerModule } from '../shared'
import { SettingsPageComponent } from './settings-page';
import { GeneralPageComponent } from './general-page';
import { OverlayPageComponent } from './overlay-page';

@NgModule({
    imports: [BrowserModule, FormsModule, MdButtonModule, MdInputModule, MdSlideToggleModule, MdTabsModule, CenterSpinnerModule],
    exports: [SettingsPageComponent],
    declarations: [SettingsPageComponent, GeneralPageComponent, OverlayPageComponent],
    entryComponents: []
})
export class SettingsModule { }