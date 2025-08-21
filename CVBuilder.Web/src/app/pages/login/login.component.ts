import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginRequest } from '../../models';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
})
export class LoginComponent {
  loginForm: FormGroup;
  errorMessage: string | null = null;
  showPassword = false;
  isSubmitting = false;

  @Output() switchToRegister = new EventEmitter<void>();

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.loginForm.invalid || this.isSubmitting) return;

    const payload: LoginRequest = {
      email: this.loginForm.value.email,
      password: this.loginForm.value.password,
    };

    this.errorMessage = null;
    this.isSubmitting = true;

    this.authService.login(payload).subscribe({
      next: (res) => {
        this.router.navigate(['/']);
      },
      error: (error) => {
        if (error?.error?.errors) {
          const messages = Object.values(error.error.errors).flat();
          this.errorMessage = String(messages.join(' '));
        } else if (error?.error?.title) {
          this.errorMessage = error.error.title;
        } else if (typeof error?.error === 'string') {
          this.errorMessage = error.error;
        } else {
          this.errorMessage = 'Unexpected error occurred.';
        }
        this.loginForm.get('password')?.reset();
      },
      complete: () => (this.isSubmitting = false),
    });
  }
}
