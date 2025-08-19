import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html',
})
export class RegisterComponent {
  registerForm: FormGroup;
  errorMessage: string | null = null;

  @Output() switchToLogin = new EventEmitter<void>();

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.fb.group(
      {
        firstName: ['', Validators.required],
        lastName: ['', Validators.required],
        email: ['', [Validators.required, Validators.email]],
        password: [
          '',
          [
            Validators.required,
            Validators.minLength(8),
            Validators.pattern('^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$'),
          ],
        ],
        confirmPassword: ['', Validators.required],
      },
      { validator: this.passwordMatchValidator }
    );
  }

  passwordMatchValidator(form: FormGroup) {
    return form.get('password')!.value === form.get('confirmPassword')!.value
      ? null
      : { mismatch: true };
  }

  get passwordsDoNotMatch() {
    return (
      this.registerForm.hasError('mismatch') &&
      this.registerForm.get('confirmPassword')!.touched
    );
  }

  onSubmit() {
    if (this.registerForm.invalid) return;

    const { confirmPassword, ...formValue } = this.registerForm.value;

    this.authService.register(formValue).subscribe({
      next: (response) => {
        const token = response.token;
        if (token) {
          localStorage.setItem('token', token);
          this.router.navigate(['/']);
        } else {
          this.errorMessage = 'Registration succeeded but token is missing.';
        }
      },
      error: (error) => {
        if (error.error && error.error.errors) {
          const messages = Object.values(error.error.errors).flat();
          this.errorMessage = messages.join(' ');
        } else if (error.error && error.error.title) {
          this.errorMessage = error.error.title;
        } else {
          this.errorMessage = 'Unexpected error occurred.';
        }
      },
    });
  }
}
