import { Injectable } from '@angular/core';
import { Http, Headers } from '@angular/http';

import 'rxjs/add/operator/map'

@Injectable()
export class HttpService {
    private URL = "http://localhost:5000";
    private headers: Headers;

    constructor(private http: Http) {
        this.headers = new Headers();
        this.headers.append('Content-Type', 'application/json');
    }

    public get<T>(api: string) {
        return this.http.get(this.URL + api, {
            headers: this.headers
        }).map(r => <T>r.json());
    }

    public put(api: string, payload?: any) {
        return this.http.put(this.URL + api, payload ? JSON.stringify(payload) : null, {
            headers: this.headers
        });
    }

    public delete(api: string) {
        return this.http.delete(this.URL + api, {
            headers: this.headers
        });
    }

    public putAndReadResponse<T>(api: string, payload?: any) {
        return this.http.put(this.URL + api, payload ? JSON.stringify(payload) : null, {
            headers: this.headers
        }).map(r => <T>r.json());
    }

    public putAndReadRawResponse<T>(api: string, payload?: any) {
        return this.http.put(this.URL + api, payload ? JSON.stringify(payload) : null, {
            headers: this.headers
        });
    }

}