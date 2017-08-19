import { NgModule } from '@angular/core';
import { MdButtonModule, MdDialogModule } from '@angular/material';

import { UpdateAvailableComponent } from './update-available-dialog.component';

@NgModule({
    imports: [MdButtonModule, MdDialogModule],
    exports: [UpdateAvailableComponent],
    declarations: [UpdateAvailableComponent],
    entryComponents: [UpdateAvailableComponent]
})
export class UpdateAvailableModule { }