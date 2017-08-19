import { NgModule } from '@angular/core';
import { MdButtonModule, MdDialogModule } from '@angular/material';

import { ConfirmationDialogComponent } from './confirmation-dialog.component';

@NgModule({
    imports: [MdButtonModule, MdDialogModule],
    exports: [ConfirmationDialogComponent],
    declarations: [ConfirmationDialogComponent],
    entryComponents: [ConfirmationDialogComponent]
})
export class ConfirmationDialogModule { }