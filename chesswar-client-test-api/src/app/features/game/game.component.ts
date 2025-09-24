import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { NgIf, NgFor } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { GameViewModel } from './game.view-model';

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [NgIf, NgFor],
  template: `
    <h2>Game Session: {{ gameId() }}</h2>
    <p *ngIf="vm.error()" style="color: red;">{{ vm.error() }}</p>
    <p *ngIf="vm.isLoading()">Загрузка...</p>
    <div *ngIf="vm.session() as s">
      <div>Мана: {{ vm.manaText() }}</div>
      <button type="button" (click)="vm.endTurn(gameId())" [disabled]="vm.isLoading()">Завершить ход</button>
      <div *ngIf="vm.board() as b" style="margin-top: 12px; display: flex; gap: 16px; align-items: flex-start;">
        <div style="display: grid; gap: 2px; width: fit-content;" [style.gridTemplateColumns]="'repeat(' + (b.size || b.width || 8) + ', 40px)'">
          <ng-container *ngFor="let y of [].constructor(b.size || b.height || 8); let row = index">
            <ng-container *ngFor="let x of [].constructor(b.size || b.width || 8); let col = index">
              <div (click)="onCellClick(col, row)" [style.width.px]="40" [style.height.px]="40" [style.display]="'flex'" [style.alignItems]="'center'" [style.justifyContent]="'center'" [style.background]="cellBg(col, row)" [style.cursor]="'pointer'" [style.color]="pieceColor(col, row)" [style.fontWeight]="pieceWeight(col, row)">
                {{ pieceGlyph(col, row) }}
              </div>
            </ng-container>
          </ng-container>
        </div>
        <div style="min-width: 220px;">
          <h4>Способности</h4>
          <div *ngIf="vm.selectedPiece() as p; else noPiece">
            <button *ngFor="let a of vm.getAbilitiesForSelected()" type="button" style="display:block; margin:4px 0;" (click)="vm.showAbilityTargets(gameId(), a.name)" [disabled]="vm.isLoading() || (a.cooldown ?? 0) > 0">
              {{ a.name }} ({{ a.manaCost }} MP) <span *ngIf="(a.cooldown ?? 0) > 0">CD {{ a.cooldown }}</span>
            </button>
          </div>
          <ng-template #noPiece>
            <div style="color:#666;">Выберите фигуру</div>
          </ng-template>
        </div>
      </div>
      <!-- Диалог эволюции -->
      <div *ngIf="vm.isEvolutionDialogOpen()" style="position: fixed; inset: 0; background: rgba(0,0,0,0.4); display: flex; align-items: center; justify-content: center;">
        <div style="background: #fff; padding: 16px; border-radius: 8px; min-width: 280px;">
          <h3>Эволюция пешки</h3>
          <p>Во что эволюционировать пешку?</p>
          <div style="display: flex; gap: 8px;">
            <button type="button" (click)="vm.confirmEvolution(gameId(), 'Knight')" [disabled]="vm.isLoading()">Конь</button>
            <button type="button" (click)="vm.confirmEvolution(gameId(), 'Bishop')" [disabled]="vm.isLoading()">Слон</button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class GameComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  readonly vm = inject(GameViewModel);
  readonly gameId = computed(() => this.route.snapshot.paramMap.get('id') ?? '');

  ngOnInit(): void {
    const id = this.gameId();
    if (id) {
      void this.vm.load(id);
    }
  }

  onCellClick(x: number, y: number): void {
    // Если выбрана способность — используем её по клику на клетку
    if (this.vm.selectedAbility()) {
      void this.vm.useAbility(this.gameId(), { x, y });
      return;
    }
    const attacks = this.vm.highlightedAttacks();
    const isAttack = attacks.some(c => c.x === x && c.y === y);
    if (isAttack) {
      void this.vm.attackTarget(this.gameId(), { x, y });
      return;
    }
    const hi = this.vm.highlighted();
    const isHighlighted = hi.some(c => c.x === x && c.y === y);
    if (isHighlighted) {
      void this.vm.moveSelectedTo(this.gameId(), { x, y });
      return;
    }
    const b = this.vm.board();
    if (!b) { return; }
    const piece = b.pieces.find(p => (p as any).position?.x === x && (p as any).position?.y === y);
    if (piece && this.vm.isPlayersPiece(piece as any)) {
      void this.vm.selectPiece(this.gameId(), String(piece.id));
      this.vm.checkEvolutionNeed(piece as any);
    }
  }

  cellBg(x: number, y: number): string {
    const idx = (x + y) % 2;
    const base = idx === 0 ? '#eee' : '#bbb';
    const isMove = this.vm.highlighted().some(c => c.x === x && c.y === y);
    const isAttack = this.vm.highlightedAttacks().some(c => c.x === x && c.y === y);
    if (isAttack) return '#ff8a80'; // красный для атак
    if (isMove) return '#90ee90';  // зелёный для ходов
    return base;
  }

  pieceGlyph(x: number, y: number): string {
    const b = this.vm.board();
    const piece = b?.pieces.find(p => (p as any).position?.x === x && (p as any).position?.y === y);
    if (!piece) { return ''; }
    const t = (piece as any).type;
    const typeName = typeof t === 'number' ? this.mapEnumToName(t) : String(t);
    switch (typeName) {
      case 'Pawn': return 'P';
      case 'Knight': return 'N';
      case 'Bishop': return 'B';
      case 'Rook': return 'R';
      case 'Queen': return 'Q';
      case 'King': return 'K';
      default: return '?';
    }
  }

  pieceColor(x: number, y: number): string {
    const b = this.vm.board();
    const s = this.vm.session();
    const piece = b?.pieces.find(p => (p as any).position?.x === x && (p as any).position?.y === y);
    if (!piece || !s) return '#111';
    const isMine = this.vm.isPlayersPiece(piece as any);
    return isMine ? '#1a73e8' : '#222';
  }

  pieceWeight(x: number, y: number): string {
    const b = this.vm.board();
    const piece = b?.pieces.find(p => (p as any).position?.x === x && (p as any).position?.y === y);
    if (!piece) return '400';
    const isMine = this.vm.isPlayersPiece(piece as any);
    return isMine ? '700' : '500';
  }

  private mapEnumToName(enumValue: number): string {
    const map: Record<number, string> = { 0: 'Pawn', 1: 'Knight', 2: 'Bishop', 3: 'Rook', 4: 'Queen', 5: 'King' };
    return map[enumValue] ?? String(enumValue);
  }
}


