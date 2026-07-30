import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ImportacionComponent } from '../../components/sicotyc/importacion/importacion.component';
import { ExportacionComponent } from '../../components/sicotyc/exportacion/exportacion.component';
import { CargaSueltaComponent } from '../../components/sicotyc/carga-suelta/carga-suelta.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ImportacionComponent, ExportacionComponent, CargaSueltaComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  selectedService: 'default' | 'importacion' | 'exportacion' | 'carga-suelta' = 'importacion';

  onServiceChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedService = (target.value as 'default' | 'importacion' | 'exportacion' | 'carga-suelta');
  }
}
