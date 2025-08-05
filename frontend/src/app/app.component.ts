import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { LoanService, Loan } from './services/loan.service';
import { AuthService } from './services/auth.service';
import { User } from './models/auth.models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  displayedColumns: string[] = [
    'amount',
    'currentBalance',
    'applicantName',
    'status',
  ];
  loans: Loan[] = [];
  loading = true;
  error: string | null = null;
  currentUser: User | null = null;

  constructor(private loanService: LoanService, private authService: AuthService) {}

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
      if (user) {
        this.loadLoans();
      }
    });
  }

  loadLoans() {
    this.loading = true;
    this.error = null;
    
    this.loanService.getLoans().subscribe({
      next: (loans) => {
        this.loans = loans;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading loans:', error);
        this.error = 'Failed to load loans. Please try again later.';
        this.loading = false;
      }
    });
  }

  logout() {
    this.authService.logout();
  }
}
