import { Component } from '@angular/core';
import { FiltroSolicitudServicio } from '../../components/filtro-solicitud-servicio/filtro-solicitud-servicio';
import { FiltroSolicitudServicioDetalle } from '../../components/filtro-solicitud-servicio-detalle/filtro-solicitud-servicio-detalle';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [FiltroSolicitudServicio, FiltroSolicitudServicioDetalle],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent {}
