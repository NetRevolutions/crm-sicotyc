import { Component, EventEmitter, inject, Output } from '@angular/core';
import { MatIcon, MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';
import { BURGUER_MENU } from '../../shared/constans/icon';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [MatIcon],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {
  @Output() menuToggle = new EventEmitter<void>();

  onToggleMenu(): void {
    this.menuToggle.emit();
  }
    constructor(){
    const iconRegistry = inject(MatIconRegistry);
    const sanitizer = inject(DomSanitizer);
    iconRegistry.addSvgIconLiteral('burguer-menu', sanitizer.bypassSecurityTrustHtml(BURGUER_MENU));

  }
}
