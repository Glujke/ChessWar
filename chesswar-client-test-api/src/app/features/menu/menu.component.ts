import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MenuViewModel } from './menu.view-model';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="menu">
      <h1>Chess War</h1>
      <button type="button" [disabled]="!vm.isTutorialEnabled()" (click)="vm.onClickTutorial()">
        Tutorial
      </button>
      <button type="button" disabled>Online (coming soon)</button>
      <button type="button" disabled>Local Multiplayer (coming soon)</button>
    </div>
  `,
})
export class MenuComponent {
  readonly vm = inject(MenuViewModel);
}


