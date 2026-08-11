import { Service, signal } from '@angular/core';

@Service()
export class NavbarService {
  isOptionOpen = signal<boolean>(false);
  isSideBarOpem = signal<boolean>(false);


}
