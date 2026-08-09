import { Component } from '@angular/core';

@Component({
  selector: 'app-filtro-solicitud-servicio-detalle',
  imports: [],
  templateUrl: './filtro-solicitud-servicio-detalle.html',
  styles: ``,
})
export class FiltroSolicitudServicioDetalle {
  detalleDelServicio: string = 'Importacion - Contenedor - Dry - 40ft';
  numeroSolicitud: string = 'S001-2026-0001'; //TODO: Evaluar si mas adelante la nomemclatura sea basado en el tipo de servicio
  fechaServicio: string = new Date().toISOString().split('T')[0]; //TODO: Evaluar si mas adelante la fecha de servicio sea basado en el tipo de servicio
}
