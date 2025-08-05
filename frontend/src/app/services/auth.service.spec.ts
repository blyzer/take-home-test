import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';

describe('AuthService', () => {
  let service: AuthService;
  let httpTestingController: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AuthService,
        { provide: Router, useValue: { navigate: jasmine.createSpy('navigate') } }
      ]
    });
    service = TestBed.inject(AuthService);
    httpTestingController = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should set currentUser$ and isAuthenticated to true on successful login', () => {
    const mockResponse = { token: 'fake-token', username: 'testuser', roles: ['User'] };
    service.login('testuser', 'password').subscribe(() => {
      expect(service.isAuthenticated).toBeTrue();
      expect(service.currentUser$.value).toEqual({ username: 'testuser', roles: ['User'] });
      expect(router.navigate).toHaveBeenCalledWith(['/']);
    });

    const req = httpTestingController.expectOne('http://localhost:5001/auth/login');
    expect(req.request.method).toEqual('POST');
    req.flush(mockResponse);
  });

  it('should clear currentUser$ and isAuthenticated on logout', () => {
    // Simulate a logged-in state
    service['currentUserSubject'] = new BehaviorSubject({ username: 'testuser', roles: ['User'] });
    service['isAuthenticatedSubject'] = new BehaviorSubject(true);

    service.logout();

    expect(service.isAuthenticated).toBeFalse();
    expect(service.currentUser$.value).toBeNull();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});