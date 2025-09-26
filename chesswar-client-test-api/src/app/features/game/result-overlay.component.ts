import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-result-overlay',
  standalone: true,
  imports: [NgIf],
  template: `
    <div *ngIf="visible" style="position: fixed; inset: 0; background: rgba(0,0,0,0.5); display:flex; align-items:center; justify-content:center;">
      <div style="background:#fff; padding:20px; border-radius:10px; min-width:320px; text-align:center;">
        <h2>{{ isWin ? 'Победа!' : 'Поражение' }}</h2>
        <div style="margin-top:12px; display:flex; gap:8px; justify-content:center;">
          <button type="button" (click)="toMenu.emit()">В меню</button>
          <button type="button" (click)="replay.emit()">Сыграть ещё раз</button>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ResultOverlayComponent {
  @Input() visible: boolean = false;
  @Input() isWin: boolean = false;
  @Output() toMenu = new EventEmitter<void>();
  @Output() replay = new EventEmitter<void>();
}


