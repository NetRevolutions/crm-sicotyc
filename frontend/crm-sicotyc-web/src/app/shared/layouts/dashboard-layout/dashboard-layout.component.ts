import { Component, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SidebarComponent } from '../../sidebar/sidebar.component';
import { HeaderComponent } from '../../header/header.component';

@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [RouterModule, SidebarComponent, HeaderComponent],
  templateUrl: './dashboard-layout.component.html',
  styleUrls: ['./dashboard-layout.component.scss']
})
export class DashboardLayoutComponent {
  readonly isSidebarOpen = signal(false);

  toggleSidebar(): void {
    this.isSidebarOpen.update((current) => !current);
  }

  closeSidebar(): void {
    this.isSidebarOpen.set(false);
  }
}
