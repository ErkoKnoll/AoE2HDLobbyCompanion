import { Component, Input } from '@angular/core';

@Component({
    selector: 'center-spinner',
    templateUrl: './center-spinner.component.html'
})
export class CenterSpinnerComponent {
    @Input() busyMessage: string;
}
