import { Component, EventEmitter, Output } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ValidatorFn,
  AbstractControl,
} from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { RegisterRequest } from '../../models';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html',
})
export class RegisterComponent {
  registerForm: FormGroup;
  errorMessage: string | null = null;
  isSubmitting = false;

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
      {
        validators: this.passwordsMatchValidator('password', 'confirmPassword'),
      }
    );
  }

  private passwordsMatchValidator(
    passwordKey: string,
    confirmKey: string
  ): ValidatorFn {
    return (group: AbstractControl) => {
      const pass = group.get(passwordKey)?.value;
      const confirm = group.get(confirmKey)?.value;
      return pass === confirm ? null : { mismatch: true };
    };
  }

  get passwordsDoNotMatch(): boolean {
    return (
      this.registerForm.hasError('mismatch') &&
      this.registerForm.get('confirmPassword')!.touched
    );
  }

  onSubmit(): void {
    if (this.registerForm.invalid || this.isSubmitting) return;

    const { confirmPassword, ...raw } = this.registerForm.value;
    const payload: RegisterRequest = {
      firstName: raw.firstName,
      lastName: raw.lastName,
      email: raw.email,
      password: raw.password,
    };

    this.errorMessage = null;
    this.isSubmitting = true;

    this.authService
      .register(payload)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: () => this.router.navigate(['/']),
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
        },
      });
  }
}
