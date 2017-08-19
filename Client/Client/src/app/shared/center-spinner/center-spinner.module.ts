import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { MdProgressSpinnerModule } from '@angular/material';

import { CenterSpinnerComponent } from './center-spinner.component';

@NgModule({
    imports: [BrowserModule, MdProgressSpinnerModule],
    exports: [CenterSpinnerComponent],
    declarations: [CenterSpinnerComponent],
})
export class CenterSpinnerModule { }
