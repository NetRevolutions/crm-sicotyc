import { Routes } from '@angular/router';
import { DashboardLayoutComponent } from './shared/layouts/dashboard-layout/dashboard-layout.component';
import { DashboardComponent } from './sicotyc/pages/dashboard/dashboard.component';
import { ClientesComponent } from './sicotyc/pages/clientes/clientes.component';
import { ReportesComponent } from './sicotyc/pages/reportes/reportes.component';
import { ConfiguracionComponent } from './sicotyc/pages/configuracion/configuracion.component';

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
  { path: '**', redirectTo: 'dashboard' }
];
