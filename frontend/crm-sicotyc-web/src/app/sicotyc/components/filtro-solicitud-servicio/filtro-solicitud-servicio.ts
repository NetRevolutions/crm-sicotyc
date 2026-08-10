import { CommonModule } from '@angular/common';
import { Component, OnInit, output, signal } from '@angular/core';
import { IFiltroSolicitudServicio } from '../../interfaces/filtro-solicitud-servicio.interface';

@Component({
  selector: 'app-filtro-solicitud-servicio',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './filtro-solicitud-servicio.html',
})
export class FiltroSolicitudServicio implements OnInit {
  readonly filtroChange = output<IFiltroSolicitudServicio>();

  selectedTipoServicio: 'default' | 'importacion' | 'exportacion' | 'traslado-interno' | 'traccion' | 'devolucion-vacios' = 'importacion';
  selectedTipoCarga: 'default' | 'contenedor' | 'carga-suelta' | 'maquinaria-pesada' | 'carga-sobredimensionada' | 'proyecto-especial' = 'default';
  selectedTipoContenedor: 'default' | 'dry' | 'reefer' | 'open-top' | 'flat-rack' | 'tank' | 'high-cube' = 'default';
  selectedTamanioContenedor: '20ft' | '40ft' = '20ft';
  selectedTipoMaquinariaPesada: 'default' | 'retro-excavadora' | 'excavadora' | 'cargador-frontal' | 'rodillo' | 'motoniveladora' | 'tractor' | 'otra' = 'default';
  selectedTipoCargaSobredimensionada: 'default' | 'estructuras-metalicas' | 'transformadores' | 'tuberias' | 'tanques' | 'bobinas' | 'otra' = 'default';
  selectedTipoFurgon: 'default' | 'furgon-ala-gaviota' | 'furgon-cerrado' | 'furgon-puerta-lateral' | 'furgon-rebatible' | 'furgon-refrigerado' | 'furgon-ventilado' | 'furgon-cisterna' | 'otra' = 'default';
  selectedRequeridos: Array<'plataforma' | 'cama-baja' | 'grua' | 'escolta' | 'permiso-mtc' | 'furgon'> = [];
  readonly filtroActual = signal<IFiltroSolicitudServicio>({
    tipoServicio: this.selectedTipoServicio,
    tipoCarga: this.selectedTipoCarga,
    requeridos: [],
  });

  private emitirFiltro(): void {
    const filtro = this.construirFiltroSeleccionado();
    this.filtroActual.set(filtro);
    this.filtroChange.emit(filtro);
  }

  private construirFiltroSeleccionado(): IFiltroSolicitudServicio {
    return {
      tipoServicio: this.selectedTipoServicio,
      tipoCarga: this.selectedTipoCarga,
      tipoContenedor: this.selectedTipoContenedor !== 'default' ? this.selectedTipoContenedor : undefined,
      tamanioContenedor: this.selectedTipoContenedor !== 'default' ? this.selectedTamanioContenedor : undefined,
      tipoMaquinariaPesada: this.selectedTipoMaquinariaPesada !== 'default' ? this.selectedTipoMaquinariaPesada : undefined,
      tipoCargaSobredimensionada: this.selectedTipoCargaSobredimensionada !== 'default' ? this.selectedTipoCargaSobredimensionada : undefined,
      tipoFurgon: this.selectedTipoFurgon !== 'default' ? this.selectedTipoFurgon : undefined,
      requeridos: [...this.selectedRequeridos],
    };
  }

  ngOnInit(): void {
    this.emitirFiltro();
  }

  onChangeTipoServicio(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedTipoServicio = (target.value as 'default' | 'importacion' | 'exportacion' | 'traslado-interno' | 'traccion' | 'devolucion-vacios');
    if (this.selectedTipoServicio === 'default') {
      this.selectedTipoCarga = 'default';
      this.selectedTipoContenedor = 'default';
      this.selectedTamanioContenedor = '20ft';
      this.selectedTipoMaquinariaPesada = 'default';
      this.selectedTipoCargaSobredimensionada = 'default';
      this.selectedTipoFurgon = 'default';
      this.selectedRequeridos = [];
    }
    else {
      this.selectedTipoCarga = 'default';
    }

    this.emitirFiltro();
  }

  onChangeTipoCarga(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedTipoCarga = (target.value as 'default' | 'contenedor' | 'carga-suelta' | 'maquinaria-pesada' | 'carga-sobredimensionada' | 'proyecto-especial');
    if (this.selectedTipoCarga === 'default') {
      this.selectedTipoContenedor = 'default';
      this.selectedTamanioContenedor = '20ft';
      this.selectedTipoMaquinariaPesada = 'default';
      this.selectedTipoCargaSobredimensionada = 'default';
      this.selectedTipoFurgon = 'default';
      this.selectedRequeridos = [];
    }
    else if (this.selectedTipoCarga === 'contenedor') {
      this.selectedTipoContenedor = 'default';
      this.selectedTamanioContenedor = '20ft';
      this.selectedTipoFurgon = 'default';
    }
    else if (this.selectedTipoCarga === 'carga-suelta'){
      this.selectedTipoContenedor = 'default';
      this.selectedTamanioContenedor = '20ft';
    }
    else if (this.selectedTipoCarga === 'maquinaria-pesada') {
      this.selectedTipoMaquinariaPesada = 'default';
      this.selectedTamanioContenedor = '20ft';
      this.selectedTipoFurgon = 'default';
    }
    else if (this.selectedTipoCarga === 'carga-sobredimensionada') {
      this.selectedTipoCargaSobredimensionada = 'default';
      this.selectedTamanioContenedor = '20ft';
      this.selectedTipoFurgon = 'default';
    }

    this.emitirFiltro();
  }

  onChangeTipoContenedor(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedTipoContenedor = (target.value as 'default' | 'dry' | 'reefer' | 'open-top' | 'flat-rack' | 'tank' | 'high-cube');

    if (this.selectedTipoContenedor === 'dry' || this.selectedTipoContenedor === 'reefer') {
      if (!this.selectedRequeridos.includes('plataforma')) {
        this.selectedRequeridos = [...this.selectedRequeridos, 'plataforma'];
      }
    }
    else {
      this.selectedRequeridos = this.selectedRequeridos.filter((item) => item !== 'plataforma');
    }

    if (this.selectedTipoContenedor === 'default') {
      this.selectedTamanioContenedor = '20ft';
      this.selectedTipoMaquinariaPesada = 'default';
      this.selectedTipoCargaSobredimensionada = 'default';
      this.selectedTipoFurgon = 'default';
      this.selectedRequeridos = [];
    }

    this.emitirFiltro();
  }

  onChangeTamanioContenedor(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedTamanioContenedor = (target.value as '20ft' | '40ft');

    this.emitirFiltro();
  }

  onChangeTipoMaquinariaPesada(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedTipoMaquinariaPesada = (target.value as 'default' | 'retro-excavadora' | 'excavadora' | 'cargador-frontal' | 'rodillo' | 'motoniveladora' | 'tractor' | 'otra');
    if (this.selectedTipoMaquinariaPesada === 'default') {
      this.selectedTipoCargaSobredimensionada = 'default';
      this.selectedTipoFurgon = 'default';
      this.selectedRequeridos = [];
    }

    this.emitirFiltro();
  }

  onChangeTipoCargaSobredimensionada(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedTipoCargaSobredimensionada = (target.value as 'default' | 'estructuras-metalicas' | 'transformadores' | 'tuberias' | 'tanques' | 'bobinas' | 'otra');
    if (this.selectedTipoCargaSobredimensionada === 'default') {
      this.selectedTipoFurgon = 'default';
      this.selectedRequeridos = [];
    }

    this.emitirFiltro();
  }

  onChangeTipoFurgon(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedTipoFurgon = (target.value as 'default' | 'furgon-ala-gaviota' | 'furgon-cerrado' | 'furgon-puerta-lateral' | 'furgon-rebatible' | 'furgon-refrigerado' | 'furgon-ventilado' | 'furgon-cisterna' | 'otra');
    if (this.selectedTipoFurgon === 'default') {
      this.selectedRequeridos = [];
    }

    this.emitirFiltro();
  }

  onChangeRequeridos(value: 'plataforma' | 'cama-baja' | 'grua' | 'escolta' | 'permiso-mtc' | 'furgon', checked: boolean): void {
    if (checked) {
      this.selectedRequeridos = [value];
      if (value !== 'furgon') {
        this.selectedTipoFurgon = 'default';
      }
      this.emitirFiltro();
      return;
    }

    this.selectedRequeridos = this.selectedRequeridos.filter((item) => item !== value);
    this.emitirFiltro();
  }
}
