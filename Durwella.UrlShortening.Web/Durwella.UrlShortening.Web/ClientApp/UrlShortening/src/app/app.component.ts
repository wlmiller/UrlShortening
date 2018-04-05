import { Component, TemplateRef, ViewChild, AfterViewInit } from '@angular/core';
import { BsModalService } from 'ngx-bootstrap/modal';
import { BsModalRef } from 'ngx-bootstrap/modal/bs-modal-ref.service';
import { AuthService } from './services/auth.service';
import { UrlShorteningService } from './services/url-shortening.service';
import 'rxjs/add/operator/finally';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements AfterViewInit {
  @ViewChild('loginTemplate') loginTemplate: TemplateRef<any>;
  isAuthenticated = false;
  modalRef: BsModalRef;
  userName: string;
  password: string;
  errorMessage: string;
  loginErrorMessage: string;
  longUrl: string;
  customPath: string;
  shortenedUrl: string;
  waiting = false;

  constructor(private modalService: BsModalService, public authService: AuthService, private urlShorteningService: UrlShorteningService) {
    this.authService.isAuthenticatedChange.subscribe(isAuth => {
      if (isAuth === false) {
        this.showLogin();
      }
    });
  }

  ngAfterViewInit(): void {
    if (this.authService.isAuthenticated === false) {
      setTimeout(() => this.showLogin());
    }
  }

  showLogin() {
    this.modalRef = this.modalService.show(
      this.loginTemplate,
      {
        ignoreBackdropClick: true
      });
  }

  submitLogin(userName: string, password: string) {
    this.authService.authenticate(userName, password)
      .subscribe(res => {
        if (res) {
          this.loginErrorMessage = null;
          this.modalRef.hide();
        } else {
          this.loginErrorMessage = 'Invalid login.';
        }
      });
  }

  logout() {
    this.userName = null;
    this.password = null;
    this.authService.logout();
  }

  submitLongUrl(url: string, customPath: string) {
    this.waiting = true;

    this.urlShorteningService.shorten(url, customPath)
      .finally(() => this.waiting = false)
      .subscribe(
        res => this.shortenedUrl = res.shortened,
        err => {
          this.shortenedUrl = null;
          this.errorMessage = err.hasOwnProperty('error') ? err.error : 'Unknown error';
        }
      );
  }
}
