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
    <div [style.maxWidth]="'1280px'" [style.margin]="'0 auto'" [style.padding]="'24px'">
      <div [style.background]="'#ffffff'" [style.borderRadius]="'12px'" [style.boxShadow]="'0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)'" [style.padding]="'24px'" [style.border]="'1px solid #e2e8f0'">
        <h2 [style.background]="'linear-gradient(135deg, #2563eb, #f59e0b)'" [style.webkitBackgroundClip]="'text'" [style.webkitTextFillColor]="'transparent'" [style.backgroundClip]="'text'" [style.fontSize]="'30px'" [style.fontWeight]="'bold'" [style.marginBottom]="'24px'">🎮 Chess War - {{ gameId() }}</h2>
        
        <div *ngIf="vm.error()" [style.background]="'#fef2f2'" [style.border]="'1px solid #fecaca'" [style.color]="'#b91c1c'" [style.padding]="'12px 16px'" [style.borderRadius]="'8px'" [style.marginBottom]="'16px'">
          ⚠️ {{ vm.error() }}
        </div>
        
        <div *ngIf="vm.isLoading()" [style.display]="'flex'" [style.alignItems]="'center'" [style.gap]="'8px'" [style.color]="'#2563eb'" [style.marginBottom]="'16px'">
          <div [style.animation]="'pulse 2s infinite'">⏳</div>
          <span>Загрузка...</span>
        </div>
        
        <div *ngIf="vm.gameState() === 'ai-thinking'" [style.background]="'#fffbeb'" [style.border]="'1px solid #fde68a'" [style.color]="'#92400e'" [style.padding]="'12px 16px'" [style.borderRadius]="'8px'" [style.marginBottom]="'16px'" [style.animation]="'pulse 2s infinite'">
          🤖 ИИ думает...
        </div>
        
        <div *ngIf="vm.session() as s">
          <app-game-status [gameId]="gameId()" />
          
          <div [style.display]="'flex'" [style.gap]="'12px'" [style.marginBottom]="'24px'">
            <button type="button" 
                    [style.display]="'inline-flex'" [style.alignItems]="'center'" [style.justifyContent]="'center'" [style.gap]="'8px'" [style.padding]="'8px 16px'" [style.border]="'none'" [style.borderRadius]="'8px'" [style.fontWeight]="'500'" [style.fontSize]="'14px'" [style.textDecoration]="'none'" [style.cursor]="'pointer'" [style.transition]="'all 0.2s'" [style.boxShadow]="'0 1px 2px 0 rgb(0 0 0 / 0.05)'" [style.background]="'#2563eb'" [style.color]="'white'"
                    (click)="vm.endTurn(gameId())" 
                    [disabled]="vm.isLoading() || !vm.canControlPieces()">
              ✅ Завершить ход
            </button>
            <button type="button" 
                    [style.display]="'inline-flex'" [style.alignItems]="'center'" [style.justifyContent]="'center'" [style.gap]="'8px'" [style.padding]="'8px 16px'" [style.border]="'1px solid #e2e8f0'" [style.borderRadius]="'8px'" [style.fontWeight]="'500'" [style.fontSize]="'14px'" [style.textDecoration]="'none'" [style.cursor]="'pointer'" [style.transition]="'all 0.2s'" [style.boxShadow]="'0 1px 2px 0 rgb(0 0 0 / 0.05)'" [style.background]="'#f8fafc'" [style.color]="'#1e293b'"
                    (click)="vm.enableHints(true)" 
                    [disabled]="vm.isLoading()">
              💡 Показать подсказки
            </button>
          </div>
          
          <div *ngIf="vm.board() as b" [style.display]="'flex'" [style.gap]="'24px'" [style.alignItems]="'flex-start'">
            <div [style.background]="'linear-gradient(45deg, #f0d9b5, #b58863)'" [style.borderRadius]="'12px'" [style.boxShadow]="'0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)'" [style.padding]="'16px'">
              <div [style.display]="'grid'" [style.gap]="'4px'" [style.gridTemplateColumns]="'repeat(' + (b.size || b.width || 8) + ', 48px)'">
                <ng-container *ngFor="let y of [].constructor(b.size || b.height || 8); let row = index">
                  <ng-container *ngFor="let x of [].constructor(b.size || b.width || 8); let col = index">
                    <div (click)="onCellClick(col, row)" 
                         [style.width.px]="48" 
                         [style.height.px]="48"
                         [style.display]="'flex'"
                         [style.alignItems]="'center'"
                         [style.justifyContent]="'center'"
                         [style.background]="cellBg(col, row)"
                         [style.cursor]="'pointer'"
                         [style.borderRadius]="'4px'"
                         [style.transition]="'all 0.2s'"
                         [style.position]="'relative'">
                      <div [style.display]="'flex'" [style.flexDirection]="'column'" [style.alignItems]="'center'" [style.gap]="'2px'">
                        <span [style.fontSize]="'18px'" [style.fontWeight]="'bold'" [style.color]="pieceColor(col, row)">{{ pieceGlyph(col, row) }}</span>
                        <div *ngIf="getPieceShield(col, row) > 0" [style.display]="'flex'" [style.alignItems]="'center'" [style.gap]="'2px'" [style.fontSize]="'10px'" [style.color]="'#3b82f6'" [style.fontWeight]="'bold'" [style.background]="'rgba(59, 130, 246, 0.1)'" [style.padding]="'2px 6px'" [style.borderRadius]="'4px'">
                          <span>🛡️</span>
                          <span>{{ getPieceShield(col, row) }}</span>
                        </div>
                        <div *ngIf="getPieceHp(col, row) > 0" [style.display]="'flex'" [style.alignItems]="'center'" [style.gap]="'2px'" [style.fontSize]="'10px'" [style.color]="'#ef4444'" [style.fontWeight]="'bold'" [style.background]="vm.damagePulse()[getPieceId(col, row)] ? 'rgba(239, 68, 68, 0.25)' : 'rgba(239, 68, 68, 0.1)'" [style.padding]="'2px 6px'" [style.borderRadius]="'4px'" [style.transition]="'background 0.2s ease'">
                          <span>❤</span>
                          <span>{{ getPieceHp(col, row) }}</span>
                        </div>
                        <div *ngIf="getPieceNeighbors(col, row) > 0" [style.fontSize]="'8px'" [style.color]="'#6b7280'" [style.fontWeight]="'normal'" [style.background]="'rgba(107, 114, 128, 0.1)'" [style.padding]="'1px 4px'" [style.borderRadius]="'3px'">
                          <span>👥</span>
                          <span>{{ getPieceNeighbors(col, row) }}</span>
                        </div>
                      </div>
                    </div>
                  </ng-container>
                </ng-container>
              </div>
            </div>
            
            <div [style.minWidth]="'256px'">
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
    const base = idx === 0 ? '#f0d9b5' : '#b58863';
    const isMove = this.vm.highlighted().some(c => c.x === x && c.y === y);
    const isAttack = this.vm.highlightedAttacks().some(c => c.x === x && c.y === y);
    if (isAttack) return '#ef4444'; // красный для атак
    if (isMove) return '#10b981';  // зелёный для ходов
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

  getPieceShield(x: number, y: number): number {
    const b = this.vm.board();
    const piece = b?.pieces.find(p => (p as any).position?.x === x && (p as any).position?.y === y);
    return (piece as any)?.shieldHp || 0;
  }

  getPieceNeighbors(x: number, y: number): number {
    const b = this.vm.board();
    const piece = b?.pieces.find(p => (p as any).position?.x === x && (p as any).position?.y === y);
    return (piece as any)?.neighborCount || 0;
  }

  getPieceHp(x: number, y: number): number {
    const b = this.vm.board();
    const piece = b?.pieces.find(p => (p as any).position?.x === x && (p as any).position?.y === y);
    return (piece as any)?.hp ?? 0;
  }

  getPieceId(x: number, y: number): string {
    const b = this.vm.board();
    const piece = b?.pieces.find(p => (p as any).position?.x === x && (p as any).position?.y === y);
    return String((piece as any)?.id ?? '');
  }

  isHighlighted(x: number, y: number): boolean {
    const hi = this.vm.highlighted();
    return hi.some(c => c.x === x && c.y === y);
  }

  isAttack(x: number, y: number): boolean {
    const attacks = this.vm.highlightedAttacks();
    return attacks.some(c => c.x === x && c.y === y);
  }
}


