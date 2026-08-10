export interface IFiltroSolicitudServicio {
  tipoServicio: string;
  tipoCarga: string;
  tipoContenedor?: string;
  tamanioContenedor?: string;
  tipoMaquinariaPesada?: string;
  tipoCargaSobredimensionada?: string;
  tipoFurgon?: string;
  requeridos: Array<string>;
}
