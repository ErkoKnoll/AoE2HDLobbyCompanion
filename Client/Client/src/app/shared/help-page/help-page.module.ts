import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { MdButtonModule, MdCheckboxModule } from '@angular/material';

import { HelpPageComponent } from './help-page.component';

@NgModule({
    imports: [BrowserModule, MdButtonModule, MdCheckboxModule, FormsModule],
    exports: [HelpPageComponent],
    declarations: [HelpPageComponent]
})
export class HelpPageModule { }