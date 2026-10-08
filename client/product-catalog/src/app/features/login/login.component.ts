import { Component, EventEmitter, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  @Output() readonly loggedIn = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  readonly form = this.fb.nonNullable.group({
    email: ['admin@catalog.local', [Validators.required, Validators.email]],
    password: ['Admin123!', [Validators.required, Validators.minLength(6)]]
  });

  submitting = false;
  error: string | null = null;

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting) {
      return;
    }

    this.submitting = true;
    this.error = null;
    const value = this.form.getRawValue();

    this.auth.login(value).subscribe({
      next: () => {
        this.submitting = false;
        this.loggedIn.emit();
      },
      error: (err: unknown) => {
        this.submitting = false;
        this.error = err instanceof Error ? err.message : 'Login failed.';
      }
    });
  }
}
