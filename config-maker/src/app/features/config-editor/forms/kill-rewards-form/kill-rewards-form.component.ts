import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-kill-rewards-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule
  ],
  template: `
    <div class="kill-rewards-form">
      <div class="form-header">
        <mat-icon>emoji_events</mat-icon>
        <h2>Награды за убийство</h2>
        <p>Настройте награды опыта за уничтожение врагов</p>
      </div>

      <mat-card class="rewards-card">
        <mat-card-header>
          <mat-icon mat-card-avatar>stars</mat-icon>
          <mat-card-title>Награды опыта</mat-card-title>
          <mat-card-subtitle>XP, получаемый за убийство каждого типа фигур</mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <div class="rewards-grid" [formGroup]="killRewardsForm">
            <mat-form-field appearance="outline" *ngFor="let piece of pieceTypes" class="reward-field">
              <mat-label>Награда за {{ piece.name }}</mat-label>
              <input matInput 
                     type="number" 
                     [formControlName]="piece.key"
                     [placeholder]="getDefaultReward(piece.key)"
                     min="0">
              <mat-icon matSuffix>{{ piece.icon }}</mat-icon>
              <mat-hint>{{ piece.description }}</mat-hint>
            </mat-form-field>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .kill-rewards-form {
      padding: 1rem;
    }

    .form-header {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-bottom: 1.5rem;
      padding: 1rem;
      background: linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%);
      border-radius: 8px;
      color: #8b4513;
    }

    .form-header h2 {
      margin: 0;
      font-size: 1.5rem;
      font-weight: 500;
    }

    .form-header p {
      margin: 0;
      opacity: 0.9;
      font-size: 0.9rem;
    }

    .rewards-card {
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    }

    .mat-mdc-card-header {
      background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
      margin: -16px -16px 16px -16px;
      padding: 16px;
    }

    .mat-mdc-card-title {
      font-size: 1.2rem;
      font-weight: 600;
      color: #495057;
    }

    .mat-mdc-card-subtitle {
      color: #6c757d;
      font-size: 0.9rem;
    }

    .rewards-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 1rem;
      padding: 1rem 0;
    }

    .reward-field {
      width: 100%;
    }

    .mat-mdc-form-field {
      font-size: 0.9rem;
    }

    @media (max-width: 768px) {
      .rewards-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class KillRewardsFormComponent {
  @Input() killRewardsForm!: FormGroup;

  readonly pieceTypes = [
    { key: 'pawn', name: 'Пешка', icon: 'person', description: 'Награда за базовую единицу' },
    { key: 'knight', name: 'Конь', icon: 'directions_horse', description: 'Награда за мобильную единицу' },
    { key: 'bishop', name: 'Слон', icon: 'church', description: 'Награда за дальнобойную единицу' },
    { key: 'rook', name: 'Ладья', icon: 'home', description: 'Награда за тяжелую единицу' },
    { key: 'queen', name: 'Ферзь', icon: 'star', description: 'Награда за мощную единицу' },
    { key: 'king', name: 'Король', icon: 'crown', description: 'Награда за королевскую единицу' }
  ];

  getDefaultReward(pieceKey: string): string {
    const defaults: Record<string, string> = {
      'pawn': '10',
      'knight': '20',
      'bishop': '20',
      'rook': '30',
      'queen': '50',
      'king': '100'
    };
    return defaults[pieceKey] || '0';
  }
}
