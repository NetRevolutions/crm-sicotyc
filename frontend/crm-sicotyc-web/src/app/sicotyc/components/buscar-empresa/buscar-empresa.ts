import { CommonModule } from '@angular/common';
import { Component, output } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface Empresa {
  ruc: string;
  razonSocial: string;
  condicion: string;
}

@Component({
  selector: 'app-buscar-empresa',
  imports: [CommonModule, FormsModule],
  templateUrl: './buscar-empresa.html',
  host: {
    class: 'block w-full',
  },
  styles: ``,
})
export class BuscarEmpresa {
  readonly rucChange = output<string>();

  selectedEmpresa = '';

  isSearchModalOpen = false;
  isAddModalOpen = false;

  searchRuc = '';
  searchRazonSocial = '';
  addRuc = '';

  readonly empresas: Empresa[] = [
    {
      ruc: '20601607973',
      razonSocial: 'Rhino Transport EIRL',
      condicion: 'HABIDO',
    },
    {
      ruc: '20100070970',
      razonSocial: 'Transporte Andino SAC',
      condicion: 'ACTIVO',
    },
    {
      ruc: '20450012345',
      razonSocial: 'Logistica del Pacifico SRL',
      condicion: 'ACTIVO',
    },
  ];

  filteredEmpresas: Empresa[] = [...this.empresas];

  openSearchModal(): void {
    this.isSearchModalOpen = true;
    this.filteredEmpresas = [...this.empresas];
  }

  closeSearchModal(): void {
    this.isSearchModalOpen = false;
    this.searchRuc = '';
    this.searchRazonSocial = '';
    this.filteredEmpresas = [...this.empresas];
  }

  searchEmpresas(): void {
    const rucQuery = this.searchRuc.trim().toLowerCase();
    const razonQuery = this.searchRazonSocial.trim().toLowerCase();

    this.filteredEmpresas = this.empresas.filter((empresa) => {
      const matchesRuc = !rucQuery || empresa.ruc.toLowerCase().includes(rucQuery);
      const matchesRazon =
        !razonQuery || empresa.razonSocial.toLowerCase().includes(razonQuery);

      return matchesRuc || matchesRazon;
    });
  }

  openAddModal(): void {
    this.isAddModalOpen = true;
    this.addRuc = this.searchRuc;
  }

  closeAddModal(): void {
    this.isAddModalOpen = false;
    this.addRuc = '';
  }

  addEmpresa(): void {
    const ruc = this.addRuc.trim();
    if (!ruc) {
      return;
    }

    const empresaExistente = this.empresas.find((empresa) => empresa.ruc === ruc);
    const razonSocial = empresaExistente?.razonSocial || this.searchRazonSocial.trim() || 'Empresa';

    this.selectedEmpresa = `${ruc} - ${razonSocial}`;
    this.rucChange.emit(ruc);
    this.closeAddModal();
    this.closeSearchModal();
  }

  useEmpresa(empresa: Empresa): void {
    this.selectedEmpresa = `${empresa.ruc} - ${empresa.razonSocial}`;
    this.rucChange.emit(empresa.ruc);
    this.closeSearchModal();
  }
}
