import { ChangeDetectionStrategy, Component, Input, computed, inject } from '@angular/core';
import { NgIf } from '@angular/common';
import { GameViewModel } from './game.view-model';

@Component({
  selector: 'app-game-status',
  standalone: true,
  imports: [NgIf],
  template: `
    <div *ngIf="vm.session() as s" style="display:flex; align-items:center; gap:8px; flex-wrap:wrap;">
      <div>Ход: {{ s.currentTurn?.number ?? '?' }}</div>
      <div>Чей ход: {{ vm.isMyTurn() ? 'Player' : 'AI' }}</div>
      <div>Мана: {{ vm.manaText() }}</div>
      <div style="color:#666;">Цель: {{ goalText() }}</div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GameStatusComponent {
  @Input() gameId: string = '';
  readonly vm = inject(GameViewModel);

  readonly goalText = computed(() => 'Убей вражеского короля');
}


