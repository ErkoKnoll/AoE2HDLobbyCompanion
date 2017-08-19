import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { LobbyPageComponent } from './lobby';
import { SettingsPageComponent } from './settings';

const routes: Routes = [
    {
        path: '',
        component: LobbyPageComponent,
    },
    {
        path: 'settings',
        component: SettingsPageComponent,
    }
];

@NgModule({
    imports: [RouterModule.forRoot(routes, { useHash: true })],
    exports: [RouterModule]
})
export class AppRoutingModule { }
