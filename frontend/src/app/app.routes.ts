import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guards';

export const routes: Routes = [
  {
    path: 'login',
    title: 'Iniciar sesión',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/login/login.page').then(m => m.LoginPage)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/main-layout').then(m => m.MainLayout),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'carga' },
      {
        path: 'carga',
        title: 'Carga de archivo',
        loadComponent: () => import('./features/upload/upload.page').then(m => m.UploadPage)
      },
      {
        path: 'procesar',
        title: 'Procesar información',
        loadComponent: () => import('./features/process/process.page').then(m => m.ProcessPage)
      },
      {
        path: 'cuarentena',
        title: 'Registros con errores',
        loadComponent: () => import('./features/quarantine/quarantine.page').then(m => m.QuarantinePage)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
