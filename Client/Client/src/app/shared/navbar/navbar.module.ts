import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { BrowserModule } from '@angular/platform-browser';
import { MdButtonModule, MdIconModule, MdTooltipModule } from '@angular/material';

import { NavBarComponent } from './navbar.component';

@NgModule({
    imports: [RouterModule, BrowserModule, MdButtonModule, MdIconModule, MdTooltipModule],
    exports: [NavBarComponent],
    declarations: [NavBarComponent],
})
export class NavBarModule { }