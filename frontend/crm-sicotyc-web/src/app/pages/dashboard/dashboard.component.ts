import { Component } from '@angular/core';
import { ImportacionComponent } from '../../components/sicotyc/importacion/importacion.component';
import { ExportacionComponent } from "../../components/sicotyc/exportacion/exportacion.component";
import { CargaSueltaComponent } from "../../components/sicotyc/carga-suelta/carga-suelta.component";

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [ImportacionComponent, ExportacionComponent, CargaSueltaComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {

}
