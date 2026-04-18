import { Component, OnInit, OnDestroy  } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { AuthService, UserSession } from '../../services/auth.service';
import { I18nService, Lang } from '../../services/i18n.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class HeaderComponent implements OnInit, OnDestroy {
  user: UserSession | null = null;
  showLoginForm = false;
  closingLoginForm = false;

  loginUserName = '';
  loginEmail = '';
  loginHomePage = '';
  loginErrors: { [key: string]: string } = {};

  private sub!: Subscription;

  constructor(public auth: AuthService, public i18n: I18nService) {}

  ngOnInit(): void {
    this.user = this.auth.getUser();
    this.sub = this.auth.userChanged$.subscribe(user => {
      this.user = user;
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  setLang(lang: Lang): void {
    this.i18n.setLang(lang);
  }

  openLoginForm(): void {
    this.loginUserName = '';
    this.loginEmail = '';
    this.loginHomePage = '';
    this.loginErrors = {};

    if (this.user) {
      this.loginUserName = this.user.userName;
      this.loginEmail = this.user.email;
      this.loginHomePage = this.user.homePage || '';
    }

    this.showLoginForm = true;
    this.closingLoginForm = false;
  }

  closeLoginForm(): void {
    this.closingLoginForm = true;
    setTimeout(() => {
      this.showLoginForm = false;
      this.closingLoginForm = false;
    }, 250);
  }

  validateLogin(): boolean {
    this.loginErrors = {};

    if (!this.loginUserName.trim()) {
      this.loginErrors['userName'] = this.i18n.t('userNameRequired');
    } else if (!/^[a-zA-Z0-9]+$/.test(this.loginUserName)) {
      this.loginErrors['userName'] = this.i18n.t('userNameInvalid');
    }

    if (!this.loginEmail.trim()) {
      this.loginErrors['email'] = this.i18n.t('emailRequired');
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.loginEmail)) {
      this.loginErrors['email'] = this.i18n.t('emailInvalid');
    }

    if (this.loginHomePage.trim() && !/^https?:\/\/.+/.test(this.loginHomePage)) {
      this.loginErrors['homePage'] = this.i18n.t('homePageInvalid');
    }

    return Object.keys(this.loginErrors).length === 0;
  }

  onLogin(): void {
    if (!this.validateLogin()) return;

    this.auth.login({
      userName: this.loginUserName.trim(),
      email: this.loginEmail.trim(),
      homePage: this.loginHomePage.trim() || ''
    });
    this.closeLoginForm();
  }

  onLogout(): void {
    this.auth.logout();
    this.loginUserName = '';
    this.loginEmail = '';
    this.loginHomePage = '';
  }
}
