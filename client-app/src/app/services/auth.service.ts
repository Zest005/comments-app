import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export interface UserSession {
  userName: string;
  email: string;
  homePage: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private storageKey = 'user_session';
  public userChanged$ = new Subject<UserSession | null>();

  getUser(): UserSession | null {
    const data = localStorage.getItem(this.storageKey);
    if (!data) return null;
    try {
      return JSON.parse(data);
    } catch {
      return null;
    }
  }

  login(user: UserSession): void {
    localStorage.setItem(this.storageKey, JSON.stringify(user));
    this.userChanged$.next(user);
  }

  logout(): void {
    localStorage.removeItem(this.storageKey);
    this.userChanged$.next(null);
  }

  isLoggedIn(): boolean {
    return this.getUser() !== null;
  }
}