import { Component, Inject } from '@angular/core';
import { MD_DIALOG_DATA } from '@angular/material';

@Component({
    selector: 'confirmation-dialog',
    templateUrl: './confirmation-dialog.component.html',
})
export class ConfirmationDialogComponent {
    public title: string;
    public question: string;

    constructor( @Inject(MD_DIALOG_DATA) data: ConfirmationDialogData) {
        this.title = data.title;
        this.question = data.question;
    }
}

export interface ConfirmationDialogData {
    title: string;
    question: string
}