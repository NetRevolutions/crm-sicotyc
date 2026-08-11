import { CommonModule } from '@angular/common';
import { Component, Input, signal } from '@angular/core';
import { IFiltroSolicitudServicio } from '../../interfaces/filtro-solicitud-servicio.interface';
import { BuscarEmpresa } from "../buscar-empresa/buscar-empresa";

@Component({
  selector: 'app-filtro-solicitud-servicio-detalle',
  standalone: true,
  imports: [CommonModule, BuscarEmpresa],
  templateUrl: './filtro-solicitud-servicio-detalle.html',
  styles: ``,
})
export class FiltroSolicitudServicioDetalle {
  private readonly filtroSolicitudServicioSignal = signal<IFiltroSolicitudServicio | null>(null);

  @Input()
  set filtroSolicitudServicio(value: IFiltroSolicitudServicio | null) {
    this.filtroSolicitudServicioSignal.set(value);
    this.detalleDelServicio.set(this.formatearDetalleDelServicio());
    this.actualizarTitulos();
  }

  get filtroSolicitudServicio(): IFiltroSolicitudServicio | null {
    return this.filtroSolicitudServicioSignal();
  }

  readonly detalleDelServicio = signal<string>('');
  readonly numeroSolicitud = signal<string>('S001-2026-0001'); //TODO: Evaluar si mas adelante la nomemclatura sea basado en el tipo de servicio
  readonly fechaServicio = signal<string>(new Date().toISOString().split('T')[0]); //TODO: Evaluar si mas adelante la fecha de servicio sea basado en el tipo de servicio

  readonly titleP1 = signal<string>('Punto de Origen');
  readonly titleAlmacenP1 = signal<string>('Almacén de Origen');
  readonly titleP2 = signal<string>('Ubicación del Cliente');
  readonly titleAlmacenP2 = signal<string>('Almacén del Cliente');
  readonly titleP3 = signal<string>('Devolución del Contenedor');
  readonly titleAlmacenP3 = signal<string>('Almacén de Devolución');

  readonly mostrarLogistica = signal(true);
  readonly mostrarSolicitante = signal(false);

  alternarSeccion(seccion: 'logistica' | 'solicitante'): void {
    this.mostrarLogistica.set(seccion === 'logistica');
    this.mostrarSolicitante.set(seccion === 'solicitante');
    console.log('filtroSolicitudServicio:', this.filtroSolicitudServicio); // TODO: Eliminar este console.log después de la depuración
  }

  private formatearDetalleDelServicio(): string {
    const filtro = this.filtroSolicitudServicio;

    if (!filtro) {
      return 'Sin filtros seleccionados';
    }

    const tipoServicio = this.formatearTexto(filtro.tipoServicio);
    const tipoCarga = filtro.tipoCarga !== 'default' ? this.formatearTexto(filtro.tipoCarga) : '';
    const detallesAdicionales = [
      filtro.tipoContenedor && filtro.tipoContenedor !== 'default' ? this.formatearTexto(filtro.tipoContenedor) : '',
      filtro.tamanioContenedor && filtro.tamanioContenedor !== 'default' ? filtro.tamanioContenedor : '',
      filtro.tipoMaquinariaPesada && filtro.tipoMaquinariaPesada !== 'default' ? this.formatearTexto(filtro.tipoMaquinariaPesada) : '',
      filtro.tipoCargaSobredimensionada && filtro.tipoCargaSobredimensionada !== 'default' ? this.formatearTexto(filtro.tipoCargaSobredimensionada) : '',
      filtro.requeridos.length > 0 ? filtro.requeridos.map((item) => this.formatearTexto(item)).join(', ') : '',
      filtro.tipoFurgon && filtro.tipoFurgon !== 'default' ? this.formatearTexto(filtro.tipoFurgon) : '',
    ].filter(Boolean);

    return [tipoServicio, tipoCarga, ...detallesAdicionales].filter(Boolean).join(' - ');
  }

  private formatearTexto(valor: string): string {
    return valor
      .replace(/-/g, ' ')
      .replace(/\b\w/g, (letra) => letra.toUpperCase());
  }

  private actualizarTitulos(): void {
    const filtro = this.filtroSolicitudServicio;

  }
}
