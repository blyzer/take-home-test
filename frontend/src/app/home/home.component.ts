import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSortModule, Sort } from '@angular/material/sort';
import { LoanService, Loan } from '../services/loan.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule, 
    MatTableModule, 
    MatButtonModule, 
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSortModule
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements OnInit {
  displayedColumns: string[] = [
    'amount',
    'currentBalance',
    'applicantName',
    'status',
    'actions'
  ];
  loans: Loan[] = [];
  loading = true;
  error: string | null = null;
  totalLoans = 0;
  pageSize = 10;
  pageIndex = 0;
  filter = '';
  sort = '';
  auditLogs: any[] = [];

  constructor(private loanService: LoanService) {}

  ngOnInit() {
    this.loadLoans();
  }

  loadLoans() {
    this.loading = true;
    this.error = null;
    
    this.loanService.getLoans(this.pageIndex + 1, this.pageSize, this.filter, this.sort).subscribe({
      next: (response) => {
        console.log('Loans loaded successfully:', response);
        this.loans = response.data;
        this.totalLoans = response.total;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading loans:', error);
        this.error = 'Failed to load loans. Please try again later.';
        this.loading = false;
      }
    });
  }

  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadLoans();
  }

  onSortChange(sort: Sort) {
    this.sort = sort.direction ? `${sort.active}_${sort.direction}` : '';
    this.loadLoans();
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.filter = filterValue.trim().toLowerCase();
    this.pageIndex = 0;
    this.loadLoans();
  }

  viewAuditLogs(loanId: number) {
    this.loanService.getAuditLogs(loanId).subscribe(logs => {
      this.auditLogs = logs;
    });
  }
}