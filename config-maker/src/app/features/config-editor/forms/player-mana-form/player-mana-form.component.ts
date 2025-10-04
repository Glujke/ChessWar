import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-player-mana-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatCheckboxModule,
    MatDividerModule
  ],
  template: `
    <div class="player-mana-form">
      <div class="form-header">
        <mat-icon>flash_on</mat-icon>
        <h2>Настройки маны игрока</h2>
        <p>Настройте систему маны и стоимость действий</p>
      </div>

      <mat-card class="mana-card">
        <mat-card-header>
          <mat-icon mat-card-avatar>battery_charging_full</mat-icon>
          <mat-card-title>Конфигурация маны</mat-card-title>
          <mat-card-subtitle>Базовые настройки маны для игроков</mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <div class="mana-basic" [formGroup]="playerManaForm">
            <div class="mana-grid">
              <mat-form-field appearance="outline" class="mana-field">
                <mat-label>Начальная мана</mat-label>
                <input matInput 
                       type="number" 
                       formControlName="initialMana"
                       placeholder="10"
                       min="0">
                <mat-icon matSuffix>play_arrow</mat-icon>
                <mat-hint>Начальная мана для новых игр</mat-hint>
              </mat-form-field>

              <mat-form-field appearance="outline" class="mana-field">
                <mat-label>Максимальная мана</mat-label>
                <input matInput 
                       type="number" 
                       formControlName="maxMana"
                       placeholder="50"
                       min="1">
                <mat-icon matSuffix>maximize</mat-icon>
                <mat-hint>Максимальная мана, которую может иметь игрок</mat-hint>
              </mat-form-field>

              <mat-form-field appearance="outline" class="mana-field">
                <mat-label>Восстановление маны</mat-label>
                <input matInput 
                       type="number" 
                       formControlName="manaRegenPerTurn"
                       placeholder="10"
                       min="0">
                <mat-icon matSuffix>refresh</mat-icon>
                <mat-hint>Мана, получаемая каждый ход</mat-hint>
              </mat-form-field>

              <mat-form-field appearance="outline" class="mana-field">
                <mat-label>Стоимость атаки</mat-label>
                <input matInput 
                       type="number" 
                       formControlName="attackCost"
                       placeholder="1"
                       min="0">
                <mat-icon matSuffix>bolt</mat-icon>
                <mat-hint>Стоимость маны за базовую атаку</mat-hint>
              </mat-form-field>
            </div>

            <div class="mana-options">
              <label class="checkbox-label">
                <input type="checkbox" 
                       formControlName="mandatoryAction"
                       class="custom-checkbox">
                <span class="checkbox-text">
                  <mat-icon>rule</mat-icon>
                  Обязательное действие
                  <span class="option-description">Игрок должен выполнить хотя бы одно действие за ход</span>
                </span>
              </label>
            </div>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card class="movement-costs-card">
        <mat-card-header>
          <mat-icon mat-card-avatar>directions_run</mat-icon>
          <mat-card-title>Стоимость движения</mat-card-title>
          <mat-card-subtitle>Стоимость маны за движение каждого типа фигур</mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <div class="movement-costs" [formGroup]="playerManaForm">
            <div class="costs-grid" formGroupName="movementCosts">
              <mat-form-field appearance="outline" *ngFor="let piece of pieceTypes" class="cost-field">
                <mat-label>Стоимость {{ piece.name }}</mat-label>
                <input matInput 
                       type="number" 
                       [formControlName]="piece.key"
                       [placeholder]="getDefaultCost(piece.key)"
                       min="0">
                <mat-icon matSuffix>{{ piece.icon }}</mat-icon>
                <mat-hint>{{ piece.description }}</mat-hint>
              </mat-form-field>
            </div>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .player-mana-form {
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

    .mana-card,
    .movement-costs-card {
      margin-bottom: 1.5rem;
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

    .mana-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 1rem;
      margin-bottom: 1.5rem;
    }

    .mana-field {
      width: 100%;
    }

    .mana-options {
      padding: 1rem;
      background: #f8f9fa;
      border-radius: 8px;
      border: 1px solid #e9ecef;
    }

    .option-description {
      display: block;
      font-size: 0.8rem;
      color: #6c757d;
      margin-top: 0.25rem;
    }

    .costs-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1rem;
    }

    .cost-field {
      width: 100%;
    }

    .mat-mdc-form-field {
      font-size: 0.9rem;
    }

    .mana-options {
      margin-top: 1rem;
      padding: 1rem;
      background: #f8f9fa;
      border-radius: 8px;
      border: 1px solid #e9ecef;
    }

    .checkbox-label {
      display: flex;
      align-items: flex-start;
      gap: 0.5rem;
      cursor: pointer;
      font-size: 1rem;
      line-height: 1.5;
    }

    .custom-checkbox {
      width: 18px;
      height: 18px;
      margin: 0;
      cursor: pointer;
      accent-color: #1976d2;
    }

    .checkbox-text {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .option-description {
      font-size: 0.85rem;
      color: #6c757d;
      font-style: italic;
    }


    @media (max-width: 768px) {
      .mana-grid,
      .costs-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class PlayerManaFormComponent {
  @Input() playerManaForm!: FormGroup;

  readonly pieceTypes = [
    { key: 'Pawn', name: 'Пешка', icon: 'person', description: 'Базовая единица' },
    { key: 'Knight', name: 'Конь', icon: 'directions_horse', description: 'Мобильная единица' },
    { key: 'Bishop', name: 'Слон', icon: 'church', description: 'Дальнобойная единица' },
    { key: 'Rook', name: 'Ладья', icon: 'home', description: 'Тяжелая единица' },
    { key: 'Queen', name: 'Ферзь', icon: 'star', description: 'Мощная единица' },
    { key: 'King', name: 'Король', icon: 'crown', description: 'Королевская единица' }
  ];

  getDefaultCost(pieceKey: string): string {
    const defaults: Record<string, string> = {
      'Pawn': '1',
      'Knight': '2',
      'Bishop': '3',
      'Rook': '3',
      'Queen': '4',
      'King': '4'
    };
    return defaults[pieceKey] || '1';
  }
}
