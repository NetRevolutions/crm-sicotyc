import { Routes } from '@angular/router';
import { DashboardLayoutComponent } from './layouts/dashboard-layout/dashboard-layout.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { ClientesComponent } from './pages/clientes/clientes.component';
import { ReportesComponent } from './pages/reportes/reportes.component';
import { ConfiguracionComponent } from './pages/configuracion/configuracion.component';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./layouts/auth-layout-component/auth-layout.component').then( m => m.AuthLayoutComponent)
  },
  {
    path: '',
    loadComponent: () =>  import('./layouts/dashboard-layout/dashboard-layout.component').then( (m) => m.DashboardLayoutComponent),
    children: [
        { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
        { path: 'dashboard', loadComponent: () =>  import('./pages/dashboard/dashboard.component').then((m)=>m.DashboardComponent) },
        { path: 'clientes', loadComponent: () =>  import('./pages/clientes/clientes.component').then((m)=>m.ClientesComponent) },
        { path: 'reportes', loadComponent: () =>  import('./pages/reportes/reportes.component').then((m)=>m.ReportesComponent) },
        { path: 'configuracion', loadComponent: () =>  import('./pages/configuracion/configuracion.component').then((m)=>m.ConfiguracionComponent) }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
