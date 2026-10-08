import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Session {
  accessToken: string;
  expiresAt: string;
  username: string;
  fullName: string;
}

const STORAGE_KEY = 'commerce.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly session = signal<Session | null>(this.restore());

  readonly user = computed(() => this.session());

  get token(): string | null {
    return this.session()?.accessToken ?? null;
  }

  isAuthenticated(): boolean {
    const session = this.session();
    if (!session) {
      return false;
    }
    if (new Date(session.expiresAt).getTime() <= Date.now()) {
      this.logout();
      return false;
    }
    return true;
  }

  login(username: string, password: string): Observable<Session> {
    return this.http
      .post<Session>(`${environment.apiUrl}/auth/login`, { username, password })
      .pipe(tap(session => this.store(session)));
  }

  logout(): void {
    this.session.set(null);
    try {
      localStorage.removeItem(STORAGE_KEY);
    } catch {
      return;
    }
  }

  private store(session: Session): void {
    this.session.set(session);
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    } catch {
      return;
    }
  }

  private restore(): Session | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as Session) : null;
    } catch {
      return null;
    }
  }
}
