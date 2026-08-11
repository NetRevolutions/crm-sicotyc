import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./shared/layouts/auth-layout-component/auth-layout.component').then( m => m.AuthLayoutComponent)
  },
  {
    path: '',
    loadComponent: () =>  import('./shared/layouts/dashboard-layout/dashboard-layout.component').then( (m) => m.DashboardLayoutComponent),
    children: [
        { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
        { path: 'dashboard', loadComponent: () =>  import('./sicotyc/pages/dashboard/dashboard.component').then((m)=>m.DashboardComponent) },
        { path: 'clientes', loadComponent: () =>  import('./sicotyc/pages/clientes/clientes.component').then((m)=>m.ClientesComponent) },
        { path: 'reportes', loadComponent: () =>  import('./sicotyc/pages/reportes/reportes.component').then((m)=>m.ReportesComponent) },
        { path: 'configuracion', loadComponent: () =>  import('./sicotyc/pages/configuracion/configuracion.component').then((m)=>m.ConfiguracionComponent) }
    ]
  },
  { path: 'error-400', loadComponent: () => import('./error/error-400/error-400.component').then((m) => m.Error400Component) },
  { path: 'error-403', loadComponent: () => import('./error/error-403/error-403.component').then((m) => m.Error403Component) },
  { path: 'error-404', loadComponent: () => import('./error/error-404/error-404.component').then((m) => m.Error404Component) },
  { path: 'error-500', loadComponent: () => import('./error/error-500/error-500.component').then((m) => m.Error500Component) },
  { path: 'error-503', loadComponent: () => import('./error/error-503/error-503.component').then((m) => m.Error503Component) },
  { path: '**', redirectTo: 'error-404' }
];
