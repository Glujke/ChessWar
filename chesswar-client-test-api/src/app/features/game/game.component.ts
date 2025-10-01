import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { NgIf, NgFor } from '@angular/common';
import { GameStatusComponent } from './game-status.component';
import { PieceContextComponent } from './piece-context.component';
import { ResultOverlayComponent } from './result-overlay.component';
import { ActivatedRoute } from '@angular/router';
import { GameViewModel } from './game.view-model';

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [NgIf, NgFor, GameStatusComponent, PieceContextComponent, ResultOverlayComponent],
  template: `
    <h2>Game Session: {{ gameId() }}</h2>
    <p *ngIf="vm.error()" style="color: red;">{{ vm.error() }}</p>
    <p *ngIf="vm.isLoading()">Загрузка...</p>
    
    <div *ngIf="vm.gameState() === 'ai-thinking'" style="background: #fff3cd; border: 1px solid #ffeaa7; padding: 8px; margin: 8px 0; border-radius: 4px; color: #856404;">
      🤖 ИИ думает...
    </div>
    
    <div *ngIf="vm.session() as s">
      <app-game-status [gameId]="gameId()" />
      <button type="button" (click)="vm.endTurn(gameId())" [disabled]="vm.isLoading() || !vm.canControlPieces()">Завершить ход</button>
        <button type="button" (click)="vm.enableHints(true)" [disabled]="vm.isLoading()" style="margin-left:8px;">Показать подсказки</button>
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
          <app-piece-context [gameId]="gameId()" />
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
      <app-result-overlay [visible]="vm.isGameFinished()" [isWin]="vm.gameResult() === 'Player1Victory'" (toMenu)="toMenu()" (replay)="onReplay()" />
      <!-- Подсказки обучения -->
      <div *ngIf="vm.showHints()" style="position:fixed; left:12px; bottom:12px; background:#fff; border:1px solid #ddd; padding:10px; border-radius:8px; max-width:320px;">
        <div *ngIf="vm.tutorialStep() === 1">Выбери свою фигуру. Подсказка: свои отмечены синим.</div>
        <div *ngIf="vm.tutorialStep() === 2">Сделай ход или атаку по подсветке. Цели атак красным, ходы зелёным.</div>
        <div *ngIf="vm.tutorialStep() === 3">Заверши ход кнопкой «Завершить ход», затем дождись ИИ.</div>
        <div *ngIf="vm.tutorialStep() === 4">Попробуй способность фигуры: выбери способность справа, затем цель.</div>
        <div style="margin-top:8px; display:flex; gap:8px;">
          <button type="button" (click)="vm.nextHint()">Далее</button>
          <button type="button" (click)="vm.skipHints()">Скрыть</button>
        </div>
      </div>
      <!-- Лог-панель -->
      <div style="position:fixed; right:12px; bottom:12px; width:360px; max-height:40vh; overflow:auto; background:#fff; border:1px solid #ddd; border-radius:8px;">
        <div style="display:flex; justify-content:space-between; gap:8px; padding:6px 8px; background:#f7f7f7; border-bottom:1px solid #eee;">
          <strong>Логи</strong>
          <button type="button" (click)="vm.clearLogs()">Очистить</button>
        </div>
        <div *ngFor="let l of vm.logs()" style="padding:6px 8px; border-bottom:1px dashed #eee; font-family:ui-monospace, SFMono-Regular, Consolas, Menlo, monospace; font-size:12px;">
          <span [style.color]="l.level === 'error' ? '#b00020' : (l.level === 'event' ? '#0a58ca' : '#444')">[{{ l.ts }}] {{ l.level.toUpperCase() }}</span>
          <span> {{ l.source }} — {{ l.message }}</span>
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

  toMenu(): void {
    location.href = '/';
  }

  onCellClick(x: number, y: number): void {
    if (this.vm.isGameFinished()) return; // блокируем ввод после завершения
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

  async onReplay(): Promise<void> {
    const id = await this.vm.replayTutorial(this.gameId());
    if (id) {
      location.href = `/#/gamesession/${id}`;
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


