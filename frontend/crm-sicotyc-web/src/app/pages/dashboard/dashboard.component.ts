import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ImportacionComponent } from '../../components/sicotyc/importacion/importacion.component';
import { ExportacionComponent } from '../../components/sicotyc/exportacion/exportacion.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ImportacionComponent, ExportacionComponent],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent {
  selectedTipoServicio: 'default' | 'importacion' | 'exportacion' | 'traslado-interno' | 'traccion' | 'devolucion-vacios' = 'importacion';
  selectedTipoCarga: 'default' | 'contenedor' | 'carga-suelta' | 'maquinaria-pesada' | 'carga-sobredimensionada' | 'proyecto-especial' = 'default';
  selectedTipoContenedor: 'default' | 'dry' | 'reefer' | 'open-top' | 'flat-rack' | 'tank' | 'high-cube' = 'default';
  selectedTamanioContenedor: '20ft' | '40ft' = '20ft'; //| '45ft'
  selectedTipoMaquinariaPesada: 'default' | 'retro-excavadora' | 'excavadora' | 'cargador-frontal' | 'rodillo' | 'motoniveladora' | 'tractor' | 'otra' = 'default';
  selectedTipoCargaSobredimensionada: 'default' | 'estructuras-metalicas' | 'transformadores' | 'tuberias' | 'tanques' | 'bobinas' | 'otra' = 'default';
  selectedTipoFurgon: 'default' | 'furgon-ala-gaviota' | 'furgon-cerrado' | 'furgon-puerta-lateral' | 'furgon-rebatible' | 'furgon-refrigerado' | 'furgon-ventilado' | 'furgon-cisterna' | 'otra' = 'default';
  selectedRequeridos: Array<'plataforma' | 'cama -baja' | 'grua' | 'escolta' | 'permiso-mtc' | 'furgon'> = [];


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
    }
    else if (this.selectedTipoCarga === 'maquinaria-pesada') {
      this.selectedTipoMaquinariaPesada = 'default';
    }
    else if (this.selectedTipoCarga === 'carga-sobredimensionada') {
      this.selectedTipoCargaSobredimensionada = 'default';
    }
  }

  onChangeTipoContenedor(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedTipoContenedor = (target.value as 'default' | 'dry' | 'reefer' | 'open-top' | 'flat-rack' | 'tank' | 'high-cube');
    if (this.selectedTipoContenedor === 'default') {
      this.selectedTamanioContenedor = '20ft';
      this.selectedTipoMaquinariaPesada = 'default';
      this.selectedTipoCargaSobredimensionada = 'default';
      this.selectedTipoFurgon = 'default';
      this.selectedRequeridos = [];
    }
  }

  onChangeTamanioContenedor(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedTamanioContenedor = (target.value as '20ft' | '40ft'); // | '45ft'
  }

  onChangeTipoMaquinariaPesada(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedTipoMaquinariaPesada = (target.value as 'default' | 'retro-excavadora' | 'excavadora' | 'cargador-frontal' | 'rodillo' | 'motoniveladora' | 'tractor' | 'otra');
    if (this.selectedTipoMaquinariaPesada === 'default') {
      this.selectedTipoCargaSobredimensionada = 'default';
      this.selectedTipoFurgon = 'default';
      this.selectedRequeridos = [];
    }
  }

  onChangeTipoCargaSobredimensionada(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedTipoCargaSobredimensionada = (target.value as 'default' | 'estructuras-metalicas' | 'transformadores' | 'tuberias' | 'tanques' | 'bobinas' | 'otra');
    if (this.selectedTipoCargaSobredimensionada === 'default') {
      this.selectedTipoFurgon = 'default';
      this.selectedRequeridos = [];
    }
  }

  onChangeTipoFurgon(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedTipoFurgon = (target.value as 'default' | 'furgon-ala-gaviota' | 'furgon-cerrado' | 'furgon-puerta-lateral' | 'furgon-rebatible' | 'furgon-refrigerado' | 'furgon-ventilado' | 'furgon-cisterna' | 'otra');
    if (this.selectedTipoFurgon === 'default') {
      this.selectedRequeridos = [];
    }
  }

  onChangeRequeridos(value: 'plataforma' | 'cama -baja' | 'grua' | 'escolta' | 'permiso-mtc' | 'furgon', checked: boolean): void {
    if (checked) {
      if (!this.selectedRequeridos.includes(value)) {
        this.selectedRequeridos = [...this.selectedRequeridos, value];
      }
      return;
    }

    this.selectedRequeridos = this.selectedRequeridos.filter((item) => item !== value);
  }

}
