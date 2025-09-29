import { ChangeDetectionStrategy, Component, Input, inject } from '@angular/core';
import { NgIf, NgFor } from '@angular/common';
import { GameViewModel } from './game.view-model';

@Component({
  selector: 'app-piece-context',
  standalone: true,
  imports: [NgIf, NgFor],
  template: `
    <div *ngIf="vm.selectedPiece() as p; else noPiece">
      <h4>Фигура</h4>
      <div>Тип: {{ typeName(p) }}</div>
      <div>HP: {{ hpOf(p) }}</div>
      <div>ATK: {{ atkOf(p) }}</div>
      <div>Move: {{ moveOf(p) }}</div>
      <div>Range: {{ rangeOf(p) }}</div>
      <h4 style="margin-top:12px;">Способности</h4>
      <button *ngFor="let a of vm.getAbilitiesForSelected(); trackBy: trackAbility" type="button" style="display:block; margin:4px 0;"
              (click)="vm.showAbilityTargets(gameId, a.name)" [disabled]="(a.cooldown ?? 0) > 0">
        {{ a.name }} ({{ a.manaCost }} MP) <span *ngIf="(a.cooldown ?? 0) > 0">CD {{ a.cooldown }}</span>
      </button>
    </div>
    <ng-template #noPiece>
      <div style="color:#666;">Выберите фигуру</div>
    </ng-template>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PieceContextComponent {
  @Input() gameId: string = '';
  readonly vm = inject(GameViewModel);

  trackAbility(_: number, a: { name: string }): string { return a.name; }

  typeName(p: unknown): string {
    const t = (p as any)?.type;
    if (typeof t === 'number') {
      const map: Record<number, string> = { 0: 'Pawn', 1: 'Knight', 2: 'Bishop', 3: 'Rook', 4: 'Queen', 5: 'King' };
      return map[t] ?? String(t);
    }
    return String(t ?? '?');
  }

  hpOf(p: unknown): number | string { return (p as any)?.hp ?? '?'; }
  atkOf(p: unknown): number | string { return (p as any)?.attack ?? (p as any)?.atk ?? '?'; }
  moveOf(p: unknown): number | string { return (p as any)?.movement ?? '?'; }
  rangeOf(p: unknown): number | string { return (p as any)?.range ?? '?'; }
}


