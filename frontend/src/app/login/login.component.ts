import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms'; // Import FormsModule
import { CommonModule } from '@angular/common'; // Import CommonModule

@Component({
  selector: 'app-login',
  standalone: true, // Make it a standalone component
  imports: [FormsModule, CommonModule], // Add FormsModule and CommonModule to imports
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  credentials = { username: '', password: '' };

  constructor(private authService: AuthService, private router: Router) { }

  login() {
    this.authService.login(this.credentials).subscribe(() => {
      this.router.navigate(['/']);
    });
  }
}