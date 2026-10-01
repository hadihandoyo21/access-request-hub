import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  currentUser = signal<string>('alice@example.local');

  setUser(email: string) {
    this.currentUser.set(email);
  }
}