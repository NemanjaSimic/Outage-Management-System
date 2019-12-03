import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable()
export class BaseHttpService<T>{
    baseUrl = "http://localhost:9000"
    specificUrl = ""

    constructor(private http: HttpClient) {
    }

    getAll(): Observable<T[]> {
        return this.http.get<T[]>(this.baseUrl + this.specificUrl).pipe(map(
            result => {
                return result['data'];
            }
        ));
    }

    getById(id: number): Observable<T> {
        return this.http.get<T>(this.baseUrl + this.specificUrl + `/${id}`).pipe(map(
            result => {
                return result['data'];
            }
        ));
    }

    // add(item: T): Observable<any> {
    //     return this.http.post(this.baseUrl + this.specificUrl, item).pipe(map(
    //         response => {
    //             return response;
    //         }
    //     ));
    // }

    // updateById(itemId: number, item: T): Observable<any> {
    //     return this.http.put(this.baseUrl + this.specificUrl + `/${itemId}`, item).pipe(map(
    //         response => {
    //             return response;
    //         }
    //     ));
    // }

    // deleteById(itemId: number): Observable<any> {
    //     return this.http.delete(this.baseUrl + this.specificUrl + `/${itemId}`).pipe(map(
    //         response => {
    //             return response;
    //         }
    //     ))
    // }
}