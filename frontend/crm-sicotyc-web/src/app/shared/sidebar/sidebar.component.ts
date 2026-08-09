import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import {MatIcon, MatIconRegistry } from '@angular/material/icon'
import { DomSanitizer } from '@angular/platform-browser';
import { BRIEFCASE, BURGUER_MENU, CHART, CLIPBOARD, CLOSE_SESSION, CLOSE_SIDEBAR, TOOL, WRENCH } from '../../shared/constans/icon';


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
  imports: [RouterLink, RouterLinkActive, MatIcon],
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

  constructor(){
    const iconRegistry = inject(MatIconRegistry);
    const sanitizer = inject(DomSanitizer);
    iconRegistry.addSvgIconLiteral('close-sidebar', sanitizer.bypassSecurityTrustHtml(CLOSE_SIDEBAR));
    iconRegistry.addSvgIconLiteral('close-session', sanitizer.bypassSecurityTrustHtml(CLOSE_SESSION));
    iconRegistry.addSvgIconLiteral('briefcase', sanitizer.bypassSecurityTrustHtml(BRIEFCASE));
    iconRegistry.addSvgIconLiteral('clipboard', sanitizer.bypassSecurityTrustHtml(CLIPBOARD));
    iconRegistry.addSvgIconLiteral('wrench', sanitizer.bypassSecurityTrustHtml(WRENCH));
    iconRegistry.addSvgIconLiteral('tool', sanitizer.bypassSecurityTrustHtml(TOOL));
    iconRegistry.addSvgIconLiteral('chart', sanitizer.bypassSecurityTrustHtml(CHART));

  }


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
