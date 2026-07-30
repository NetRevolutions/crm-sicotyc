import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

type UserRole = 'Administrador' | 'Supervisor';

interface SidebarChildItem {
  label: string;
  path: string;
}

interface SidebarSection {
  label: string;
  icon: 'briefcase' | 'ship' | 'box' | 'clipboard' | 'tool' | 'wrench' | 'chart';
  children?: SidebarChildItem[];
  roles: UserRole[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  @Input() isOpen = false;
  @Output() navigate = new EventEmitter<void>();

  readonly currentRole: UserRole = 'Administrador';

  // Get Dynamic Sidebar Sections based on the current role (future)
  readonly sections: SidebarSection[] = [
    {
      label:'Solicitud Servicio', //'Importacion',
      icon: 'briefcase',
      children: [
        { label: 'Landing Page', path: '/dashboard' },
        // { label: 'Solicitud Servicio', path: '/clientes' }
      ],
      roles: ['Administrador', 'Supervisor']
    },
    // { label: 'Exportacion', icon: 'ship', roles: ['Administrador', 'Supervisor'] },
    // { label: 'Carga Suelta', icon: 'box', roles: ['Administrador', 'Supervisor'] },
    { label: 'Evaluacion', icon: 'clipboard', roles: ['Administrador', 'Supervisor'] },
    { label: 'Orden de Trabajo', icon: 'wrench', roles: ['Administrador', 'Supervisor'] },
    { label: 'Mantenimientos', icon: 'tool', roles: ['Administrador'] },
    { label: 'Reportes', icon: 'chart', roles: ['Administrador', 'Supervisor'] }
  ];

  readonly expandedSections = new Set<string>(['Solicitud Servicio']);

  get visibleSections(): SidebarSection[] {
    return this.sections.filter((item) => item.roles.includes(this.currentRole));
  }

  isExpanded(sectionLabel: string): boolean {
    return this.expandedSections.has(sectionLabel);
  }

  toggleSection(sectionLabel: string): void {
    if (this.expandedSections.has(sectionLabel)) {
      this.expandedSections.delete(sectionLabel);
      return;
    }

    this.expandedSections.add(sectionLabel);
  }

  onNavigate(): void {
    this.navigate.emit();
  }

}
