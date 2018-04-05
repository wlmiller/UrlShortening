import { Injectable, EventEmitter } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import 'rxjs/add/observable/of';

@Injectable()
export class AuthService {
  token: string;
  isAuthenticatedVal = null;
  isAuthenticatedChange: EventEmitter<boolean> = new EventEmitter<boolean>();

  constructor(private http: HttpClient) {
    const token = localStorage.getItem('token');

    if (!!token) {
      this.checkAuthentication(token).subscribe(res => {
        if (res) {
          this.token = token;
        }

        this.isAuthenticated = res;
      });
    } else {
      this.isAuthenticated = false;
    }
  }

  get isAuthenticated(): boolean {
    return this.isAuthenticatedVal;
  }

  set isAuthenticated(val: boolean) {
    this.isAuthenticatedVal = val;
    this.isAuthenticatedChange.emit(val);
  }

  authenticate(userName: string, password: string) {
    const headers = new HttpHeaders()
      .set('Content-Type', 'application/x-www-form-urlencoded');

    const body = new HttpParams()
      .set('userName', userName)
      .set('password', password);

    return this.http.post<boolean>(
      '/auth/cred',
      body,
      { headers: headers }
    )
    .catch(err => Observable.of(null))
    .map(
      res => {
        if (!res) {
          return false;
        }

        this.isAuthenticated = true;
        this.token = res['access_token'];
        localStorage.setItem('token', this.token);

        return true;
      }
    );
  }

  logout() {
    this.token = null;
    localStorage.removeItem('token');

    this.isAuthenticated = false;
  }

  private checkAuthentication(token: string): Observable<boolean> {
    const headers = new HttpHeaders()
      .set('Authorization', `Bearer ${token}`);

    return this.http.get<boolean>('/auth/validate', { headers: headers })
      .catch(err => {
        this.token = null;
        localStorage.removeItem('token');

        return Observable.of(false);
      });
  }
}
