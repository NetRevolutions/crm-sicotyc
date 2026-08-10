import { Component } from '@angular/core';
import { FiltroSolicitudServicio } from '../../sicotyc/components/filtro-solicitud-servicio/filtro-solicitud-servicio';
import { FiltroSolicitudServicioDetalle } from '../../sicotyc/components/filtro-solicitud-servicio-detalle/filtro-solicitud-servicio-detalle';
import { IFiltroSolicitudServicio } from '../../sicotyc/interfaces/filtro-solicitud-servicio.interface';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [FiltroSolicitudServicio, FiltroSolicitudServicioDetalle],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent {
  filtroSolicitudServicio: IFiltroSolicitudServicio | null = null;

  onFiltroChange(filtro: IFiltroSolicitudServicio): void {
    this.filtroSolicitudServicio = filtro;
  }
}
