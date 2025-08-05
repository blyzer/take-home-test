import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { LoginRequest, RegisterRequest, AuthResponse, User, ChangePasswordRequest, UserRole } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5001/api/auth';
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private tokenSubject = new BehaviorSubject<string | null>(null);

  public currentUser$ = this.currentUserSubject.asObservable();
  public token$ = this.tokenSubject.asObservable();

  constructor(private http: HttpClient) {
    // Load user and token from localStorage on service initialization
    this.loadStoredAuth();
  }

  private loadStoredAuth(): void {
    const token = localStorage.getItem('authToken');
    const user = localStorage.getItem('currentUser');
    
    if (token && user) {
      try {
        const parsedUser = JSON.parse(user);
        this.tokenSubject.next(token);
        this.currentUserSubject.next(parsedUser);
      } catch (error) {
        // Clear invalid data
        this.logout();
      }
    }
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials)
      .pipe(
        tap(response => {
          this.storeAuth(response);
        })
      );
  }

  register(userData: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, userData)
      .pipe(
        tap(response => {
          this.storeAuth(response);
        })
      );
  }

  logout(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('currentUser');
    this.tokenSubject.next(null);
    this.currentUserSubject.next(null);
  }

  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/profile`);
  }

  changePassword(request: ChangePasswordRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/change-password`, request);
  }

  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/users`);
  }

  updateUserRole(userId: number, role: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/users/${userId}/role`, { role });
  }

  deactivateUser(userId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/users/${userId}`);
  }

  private storeAuth(response: AuthResponse): void {
    localStorage.setItem('authToken', response.token);
    localStorage.setItem('currentUser', JSON.stringify(response.user));
    this.tokenSubject.next(response.token);
    this.currentUserSubject.next(response.user);
  }

  get currentUser(): User | null {
    return this.currentUserSubject.value;
  }

  get token(): string | null {
    return this.tokenSubject.value;
  }

  get isAuthenticated(): boolean {
    return this.token !== null && this.currentUser !== null;
  }

  hasRole(role: UserRole): boolean {
    return this.currentUser?.role === role;
  }

  hasAnyRole(roles: UserRole[]): boolean {
    return roles.some(role => this.hasRole(role));
  }

  get isAdmin(): boolean {
    return this.hasRole(UserRole.Admin);
  }

  get isManager(): boolean {
    return this.hasRole(UserRole.Manager);
  }

  get isManagerOrAdmin(): boolean {
    return this.hasAnyRole([UserRole.Manager, UserRole.Admin]);
  }
}
