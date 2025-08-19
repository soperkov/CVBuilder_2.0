import { Component } from '@angular/core';

@Component({
  selector: 'app-auth-page',
  standalone: false,
  templateUrl: './auth-page.component.html',
  styleUrl: './auth-page.component.css',
})
export class AuthPageComponent {
  showLogin = true;

  toggleForm(): void {
    this.showLogin = !this.showLogin;
  }
}
