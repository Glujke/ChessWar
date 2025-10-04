import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDividerModule } from '@angular/material/divider';

interface PieceType {
  key: string;
  name: string;
  icon: string;
  description: string;
}

@Component({
  selector: 'app-pieces-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatTabsModule,
    MatDividerModule
  ],
  template: `
    <div class="pieces-form">
      <div class="form-header">
        <mat-icon>extension</mat-icon>
        <h2>Настройка фигур</h2>
        <p>Настройте характеристики для каждого типа шахматных фигур</p>
      </div>

      <mat-tab-group class="pieces-tabs" animationDuration="300ms">
        <mat-tab *ngFor="let piece of pieceTypes" [label]="piece.name">
          <ng-template matTabContent>
            <mat-card class="piece-card">
              <mat-card-header>
                <mat-icon mat-card-avatar>{{ piece.icon }}</mat-icon>
                <mat-card-title>{{ piece.name }}</mat-card-title>
                <mat-card-subtitle>{{ piece.description }}</mat-card-subtitle>
              </mat-card-header>
              
              <mat-card-content>
                <div class="piece-stats" [formGroup]="getPieceFormGroup(piece.key)">
                  <div class="stats-grid">
                    <mat-form-field appearance="outline" class="stat-field">
                      <mat-label>Очки здоровья</mat-label>
                      <input matInput 
                             type="number" 
                             formControlName="hp"
                             placeholder="10"
                             min="1">
                      <mat-icon matSuffix>favorite</mat-icon>
                    </mat-form-field>

                    <mat-form-field appearance="outline" class="stat-field">
                      <mat-label>Сила атаки</mat-label>
                      <input matInput 
                             type="number" 
                             formControlName="atk"
                             placeholder="2"
                             min="0">
                      <mat-icon matSuffix>flash_on</mat-icon>
                    </mat-form-field>

                    <mat-form-field appearance="outline" class="stat-field">
                      <mat-label>Дальность атаки</mat-label>
                      <input matInput 
                             type="number" 
                             formControlName="range"
                             placeholder="1"
                             min="0">
                      <mat-icon matSuffix>gps_fixed</mat-icon>
                    </mat-form-field>

                    <mat-form-field appearance="outline" class="stat-field">
                      <mat-label>Дальность движения</mat-label>
                      <input matInput 
                             type="number" 
                             formControlName="movement"
                             placeholder="1"
                             min="0">
                      <mat-icon matSuffix>directions_run</mat-icon>
                    </mat-form-field>

                    <mat-form-field appearance="outline" class="stat-field">
                      <mat-label>XP для эволюции</mat-label>
                      <input matInput 
                             type="number" 
                             formControlName="xpToEvolve"
                             placeholder="20"
                             min="0">
                      <mat-icon matSuffix>trending_up</mat-icon>
                    </mat-form-field>

                    <mat-form-field appearance="outline" class="stat-field">
                      <mat-label>Максимальные очки щита</mat-label>
                      <input matInput 
                             type="number" 
                             formControlName="maxShieldHP"
                             placeholder="50"
                             min="0">
                      <mat-icon matSuffix>shield</mat-icon>
                    </mat-form-field>
                  </div>
                </div>
              </mat-card-content>
            </mat-card>
          </ng-template>
        </mat-tab>
      </mat-tab-group>
    </div>
  `,
  styles: [`
    .pieces-form {
      padding: 1rem;
    }

    .form-header {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-bottom: 1.5rem;
      padding: 1rem;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 8px;
      color: white;
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

    .pieces-tabs {
      margin-top: 1rem;
    }

    .piece-card {
      margin: 1rem 0;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    }

    .piece-stats {
      padding: 1rem 0;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 1rem;
    }

    .stat-field {
      width: 100%;
    }

    .mat-mdc-form-field {
      font-size: 0.9rem;
    }

    .mat-mdc-tab-group {
      --mdc-tab-indicator-active-indicator-color: #667eea;
      --mdc-tab-text-label-text-color: #667eea;
    }

    .mat-mdc-tab-group .mat-mdc-tab {
      min-width: 120px;
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

    @media (max-width: 768px) {
      .stats-grid {
        grid-template-columns: 1fr;
      }
      
      .form-header {
        flex-direction: column;
        text-align: center;
      }
    }
  `]
})
export class PiecesFormComponent {
  @Input() piecesForm!: FormGroup;

  readonly pieceTypes: PieceType[] = [
    {
      key: 'Pawn',
      name: 'Пешка',
      icon: 'person',
      description: 'Базовая пехотная единица с ограниченным движением, но потенциалом для эволюции'
    },
    {
      key: 'Knight',
      name: 'Конь',
      icon: 'directions_horse',
      description: 'Мобильная кавалерийская единица с L-образным движением'
    },
    {
      key: 'Bishop',
      name: 'Слон',
      icon: 'church',
      description: 'Дальнобойная поддерживающая единица, движущаяся по диагонали'
    },
    {
      key: 'Rook',
      name: 'Ладья',
      icon: 'home',
      description: 'Тяжелая оборонительная единица с прямолинейным движением'
    },
    {
      key: 'Queen',
      name: 'Ферзь',
      icon: 'star',
      description: 'Самая мощная фигура с комбинированными способностями движения'
    },
    {
      key: 'King',
      name: 'Король',
      icon: 'crown',
      description: 'Королевская фигура, которую необходимо защищать любой ценой'
    }
  ];

  getPieceFormGroup(pieceKey: string): FormGroup {
    return this.piecesForm.get(pieceKey) as FormGroup;
  }
}
