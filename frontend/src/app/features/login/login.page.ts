import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { toApiErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule],
  templateUrl: './login.page.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly returnUrl = input<string>();

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly form = inject(NonNullableFormBuilder).group({
    username: ['', Validators.required],
    password: ['', Validators.required]
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { username, password } = this.form.getRawValue();
    this.loading.set(true);
    this.error.set(null);
    this.auth
      .login(username, password)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => this.router.navigateByUrl(this.returnUrl() || '/carga'),
        error: error => this.error.set(toApiErrorMessage(error).title)
      });
  }

  protected invalid(control: 'username' | 'password'): boolean {
    const c = this.form.controls[control];
    return c.invalid && c.touched;
  }
}
