import { Routes } from '@angular/router';
import { DashboardLayoutComponent } from './layouts/dashboard-layout/dashboard-layout.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { ClientesComponent } from './pages/clientes/clientes.component';
import { ReportesComponent } from './pages/reportes/reportes.component';
import { ConfiguracionComponent } from './pages/configuracion/configuracion.component';

export const routes: Routes = [
    {
        path: '',
        component: DashboardLayoutComponent,
        children: [
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            { path: 'dashboard', component: DashboardComponent },
            { path: 'clientes', component: ClientesComponent },
            { path: 'reportes', component: ReportesComponent },
            { path: 'configuracion', component: ConfiguracionComponent }
        ]
    },
    { path: '**', redirectTo: 'dashboard' }
];
