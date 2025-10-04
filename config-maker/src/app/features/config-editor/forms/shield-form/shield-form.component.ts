import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-shield-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatDividerModule
  ],
  template: `
    <div class="shield-form">
      <div class="form-header">
        <mat-icon>shield</mat-icon>
        <h2>Система щитов</h2>
        <p>Настройте механику коллективных щитов</p>
      </div>

      <mat-card class="king-shield-card">
        <mat-card-header>
          <mat-icon mat-card-avatar>crown</mat-icon>
          <mat-card-title>Конфигурация щита короля</mat-card-title>
          <mat-card-subtitle>Регенерация щита короля и бонусы</mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <div class="king-shield-settings" [formGroup]="shieldForm">
            <div formGroupName="king">
              <mat-form-field appearance="outline" class="shield-field">
                <mat-label>Базовая регенерация</mat-label>
                <input matInput 
                       type="number" 
                       formControlName="baseRegen"
                       placeholder="5"
                       min="0">
                <mat-icon matSuffix>refresh</mat-icon>
                <mat-hint>Базовая регенерация щита за ход</mat-hint>
              </mat-form-field>

              <mat-divider></mat-divider>

              <div class="proximity-bonuses">
                <h4>Бонусы близости</h4>
                <p class="bonus-description">Бонусы щита от ближайших союзников</p>
                
                <div class="bonus-section">
                  <h5>Бонус дистанции 1</h5>
                  <div class="bonus-grid" formGroupName="proximityBonus1">
                    <mat-form-field appearance="outline" *ngFor="let piece of pieceTypes" class="bonus-field">
                      <mat-label>{{ piece.name }}</mat-label>
                      <input matInput 
                             type="number" 
                             [formControlName]="piece.key"
                             [placeholder]="getDefaultBonus1(piece.key)"
                             min="0">
                      <mat-icon matSuffix>{{ piece.icon }}</mat-icon>
                    </mat-form-field>
                  </div>
                </div>

                <div class="bonus-section">
                  <h5>Бонус дистанции 2</h5>
                  <div class="bonus-grid" formGroupName="proximityBonus2">
                    <mat-form-field appearance="outline" *ngFor="let piece of pieceTypes" class="bonus-field">
                      <mat-label>{{ piece.name }}</mat-label>
                      <input matInput 
                             type="number" 
                             [formControlName]="piece.key"
                             [placeholder]="getDefaultBonus2(piece.key)"
                             min="0">
                      <mat-icon matSuffix>{{ piece.icon }}</mat-icon>
                    </mat-form-field>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card class="ally-shield-card">
        <mat-card-header>
          <mat-icon mat-card-avatar>group</mat-icon>
          <mat-card-title>Конфигурация щита союзников</mat-card-title>
          <mat-card-subtitle>Вклад обычных фигур в щит</mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <div class="ally-shield-settings" [formGroup]="shieldForm">
            <div formGroupName="ally">
              <div class="contribution-section">
                <h4>Neighbor Contribution</h4>
                <p class="contribution-description">Shield contribution from each piece type at distance ≤ 1</p>
                
                <div class="contribution-grid" formGroupName="neighborContribution">
                <mat-form-field appearance="outline" *ngFor="let piece of pieceTypes" class="contribution-field">
                  <mat-label>{{ piece.name }} Contribution</mat-label>
                  <input matInput 
                         type="number" 
                         [formControlName]="piece.key"
                         [placeholder]="getDefaultContribution(piece.key)"
                         min="0">
                  <mat-icon matSuffix>{{ piece.icon }}</mat-icon>
                  <mat-hint>{{ piece.description }}</mat-hint>
                </mat-form-field>
                </div>
              </div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .shield-form {
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

    .king-shield-card,
    .ally-shield-card {
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

    .shield-field {
      width: 100%;
      max-width: 300px;
      margin-bottom: 1rem;
    }

    .proximity-bonuses {
      margin-top: 1.5rem;
    }

    .proximity-bonuses h4 {
      margin: 0 0 0.5rem 0;
      color: #495057;
      font-size: 1.1rem;
    }

    .bonus-description {
      color: #6c757d;
      font-size: 0.9rem;
      margin-bottom: 1rem;
    }

    .bonus-section {
      margin-bottom: 1.5rem;
    }

    .bonus-section h5 {
      margin: 0 0 0.5rem 0;
      color: #495057;
      font-size: 1rem;
      font-weight: 600;
    }

    .bonus-grid,
    .contribution-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1rem;
    }

    .bonus-field,
    .contribution-field {
      width: 100%;
    }

    .contribution-section h4 {
      margin: 0 0 0.5rem 0;
      color: #495057;
      font-size: 1.1rem;
    }

    .contribution-description {
      color: #6c757d;
      font-size: 0.9rem;
      margin-bottom: 1rem;
    }

    .mat-mdc-form-field {
      font-size: 0.9rem;
    }

    @media (max-width: 768px) {
      .bonus-grid,
      .contribution-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class ShieldFormComponent {
  @Input() shieldForm!: FormGroup;

  readonly pieceTypes = [
    { key: 'pawn', name: 'Пешка', icon: 'person', description: 'Базовая единица' },
    { key: 'knight', name: 'Конь', icon: 'directions_horse', description: 'Мобильная единица' },
    { key: 'bishop', name: 'Слон', icon: 'church', description: 'Дальнобойная единица' },
    { key: 'rook', name: 'Ладья', icon: 'home', description: 'Тяжелая единица' },
    { key: 'queen', name: 'Ферзь', icon: 'star', description: 'Мощная единица' },
    { key: 'king', name: 'Король', icon: 'crown', description: 'Королевская единица' }
  ];

  getDefaultBonus1(pieceKey: string): string {
    const defaults: Record<string, string> = {
      'pawn': '1',
      'knight': '2',
      'bishop': '2',
      'rook': '3',
      'queen': '4',
      'king': '0'
    };
    return defaults[pieceKey] || '0';
  }

  getDefaultBonus2(pieceKey: string): string {
    const defaults: Record<string, string> = {
      'pawn': '0',
      'knight': '1',
      'bishop': '1',
      'rook': '2',
      'queen': '3',
      'king': '0'
    };
    return defaults[pieceKey] || '0';
  }

  getDefaultContribution(pieceKey: string): string {
    const defaults: Record<string, string> = {
      'pawn': '1',
      'knight': '2',
      'bishop': '2',
      'rook': '3',
      'queen': '4',
      'king': '5'
    };
    return defaults[pieceKey] || '0';
  }
}
