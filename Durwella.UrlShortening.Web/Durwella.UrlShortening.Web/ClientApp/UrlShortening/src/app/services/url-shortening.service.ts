import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ShortUrlResponse } from '../models/short-url-response';
import { ShortUrlRequest } from '../models/short-url-request';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class UrlShorteningService {

  constructor(private authService: AuthService, private http: HttpClient) { }

  public shorten(url: string, customPath: string = null): Observable<ShortUrlResponse> {
    const headers = new HttpHeaders()
      .set('Authorization', `Bearer ${this.authService.token}`);

    return this.http.post<ShortUrlResponse>('/',
      new ShortUrlRequest(url, customPath),
      { headers: headers }
    );
  }
}
