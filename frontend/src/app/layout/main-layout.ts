import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './main-layout.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MainLayout {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly user = this.auth.user;
  protected readonly links = [
    { path: '/carga', label: 'Carga de archivo' },
    { path: '/procesar', label: 'Procesar' },
    { path: '/cuarentena', label: 'Registros con errores' }
  ];

  protected logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
