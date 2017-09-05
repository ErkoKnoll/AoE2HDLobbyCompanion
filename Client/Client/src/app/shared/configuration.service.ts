import { Injectable } from '@angular/core';

import { HttpService } from './';
import { Configuration } from '../app.models';

@Injectable()
export class ConfigurationService {
    public configuration: Configuration;

    constructor(private httpService: HttpService) {
    }

    public getConfiguration(done: (success: boolean) => void) {
        this.httpService.get<Configuration>("/api/configuration").subscribe(configuration => {
            this.configuration = configuration;
            done(true);
        }, error => {
            console.log("Error while reading configuration", error);
            done(false);
        });
    }

    public saveConfiguration() {
        return this.httpService.put("/api/configuration", this.configuration);
    }

}