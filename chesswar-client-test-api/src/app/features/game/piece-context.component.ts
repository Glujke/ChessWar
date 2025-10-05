import { ChangeDetectionStrategy, Component, Input, inject } from '@angular/core';
import { NgIf, NgFor } from '@angular/common';
import { GameViewModel } from './game.view-model';

@Component({
  selector: 'app-piece-context',
  standalone: true,
  imports: [NgIf, NgFor],
  template: `
    <div class="card animate-slide-in">
      <div *ngIf="vm.selectedPiece() as p; else noPiece">
        <h4 class="text-xl font-bold mb-4 text-gradient">🎯 Информация о фигуре</h4>
        
        <div class="space-y-3">
          <div class="flex justify-between items-center p-3 bg-gray-50 rounded-lg">
            <span class="font-medium">Тип:</span>
            <span class="font-bold text-blue-600">{{ typeName(p) }}</span>
          </div>
          
          <div class="flex justify-between items-center p-3 bg-red-50 rounded-lg">
            <span class="font-medium">❤️ HP:</span>
            <span class="font-bold text-red-600">{{ hpOf(p) }}</span>
          </div>
          
          <div class="flex justify-between items-center p-3 bg-orange-50 rounded-lg">
            <span class="font-medium">⚔️ Атака:</span>
            <span class="font-bold text-orange-600">{{ atkOf(p) }}</span>
          </div>
          
          <div class="flex justify-between items-center p-3 bg-green-50 rounded-lg">
            <span class="font-medium">🏃 Движение:</span>
            <span class="font-bold text-green-600">{{ moveOf(p) }}</span>
          </div>
          
          <div class="flex justify-between items-center p-3 bg-purple-50 rounded-lg">
            <span class="font-medium">🎯 Дальность:</span>
            <span class="font-bold text-purple-600">{{ rangeOf(p) }}</span>
          </div>
          
          <div *ngIf="shieldOf(p) > 0" class="flex justify-between items-center p-3 bg-blue-50 rounded-lg border-2 border-blue-200">
            <span class="font-medium">🛡️ Щит:</span>
            <span class="font-bold text-blue-600">{{ shieldOf(p) }}</span>
          </div>
          
          <div *ngIf="neighborsOf(p) > 0" class="flex justify-between items-center p-3 bg-gray-50 rounded-lg">
            <span class="font-medium text-sm">👥 Соседи:</span>
            <span class="font-bold text-gray-600">{{ neighborsOf(p) }}</span>
          </div>
        </div>
        
        <h4 class="text-lg font-bold mt-6 mb-4 text-gradient">✨ Способности</h4>
        <div class="space-y-2">
          <button *ngFor="let a of vm.getAbilitiesForSelected(); trackBy: trackAbility" 
                  type="button" 
                  class="btn btn-secondary w-full text-left justify-start"
                  (click)="vm.showAbilityTargets(gameId, a.name)" 
                  [disabled]="(a.cooldown ?? 0) > 0">
            <span class="font-medium">{{ a.name }}</span>
            <span class="text-sm text-gray-500 ml-2">({{ a.manaCost }} MP)</span>
            <span *ngIf="(a.cooldown ?? 0) > 0" class="text-red-500 ml-2">CD {{ a.cooldown }}</span>
          </button>
        </div>
      </div>
      
      <ng-template #noPiece>
        <div class="text-center text-gray-500 py-8">
          <div class="text-4xl mb-2">🎯</div>
          <p>Выберите фигуру для просмотра информации</p>
        </div>
      </ng-template>
    </div>
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
  shieldOf(p: unknown): number { return (p as any)?.shieldHp ?? 0; }
  neighborsOf(p: unknown): number { return (p as any)?.neighborCount ?? 0; }
}


