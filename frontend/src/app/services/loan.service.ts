import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Loan {
  id: number;
  amount: number;
  currentBalance: number;
  applicantName: string;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class LoanService {
  private apiUrl = 'http://localhost:5001/loans';

  constructor(private http: HttpClient) { }

  getLoans(page: number = 1, pageSize: number = 10, filter: string = '', sort: string = ''): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}?page=${page}&pageSize=${pageSize}&filter=${filter}&sort=${sort}`);
  }

  getLoan(id: number): Observable<Loan> {
    return this.http.get<Loan>(`${this.apiUrl}/${id}`);
  }

  createLoan(loan: Omit<Loan, 'id'>): Observable<Loan> {
    return this.http.post<Loan>(this.apiUrl, loan);
  }

  makePayment(id: number, paymentAmount: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/payment`, paymentAmount);
  }

  getAuditLogs(id: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${id}/audit`);
  }
}
