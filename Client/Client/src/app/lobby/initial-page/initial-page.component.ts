import { Component, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'initial-page',
    templateUrl: 'initial-page.component.html'
})
export class InitialPageComponent {
    @Output() sessionStart = new EventEmitter<any>();

    public startSession() {
        this.sessionStart.emit();
    }
}
