import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FiltroSolicitudServicio } from '../../components/filtro-solicitud-servicio/filtro-solicitud-servicio';
import { FiltroSolicitudServicioDetalle } from '../../components/filtro-solicitud-servicio-detalle/filtro-solicitud-servicio-detalle';
import { IFiltroSolicitudServicio } from '../../interfaces/filtro-solicitud-servicio.interface';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FiltroSolicitudServicio, FiltroSolicitudServicioDetalle],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent {
  readonly filtroSolicitudServicio = signal<IFiltroSolicitudServicio | null>(null);
  readonly showFiltroSolicitudServicioDetalle = signal(false);

  onFiltroChange(filtro: IFiltroSolicitudServicio): void {
    this.filtroSolicitudServicio.set(filtro);
    const shouldShowDetail = filtro.tipoServicio !== 'default' && filtro.tipoCarga !== 'default';
    this.showFiltroSolicitudServicioDetalle.set(shouldShowDetail);
  }
}
