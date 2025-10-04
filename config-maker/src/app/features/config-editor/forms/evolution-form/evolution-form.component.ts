import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatAutocompleteModule } from '@angular/material/autocomplete';

interface PieceEvolution {
  key: string;
  name: string;
  icon: string;
  description: string;
}

@Component({
  selector: 'app-evolution-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatCheckboxModule,
    MatSelectModule,
    MatDividerModule,
    MatChipsModule,
    MatAutocompleteModule
  ],
  template: `
    <div class="evolution-form">
      <div class="form-header">
        <mat-icon>trending_up</mat-icon>
        <h2>Настройка эволюции</h2>
        <p>Настройте, как фигуры эволюционируют и получают опыт</p>
      </div>

      <div class="evolution-sections">
        <!-- XP Thresholds -->
        <mat-card class="evolution-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>emoji_events</mat-icon>
            <mat-card-title>XP Thresholds</mat-card-title>
            <mat-card-subtitle>Experience points required for each piece to evolve</mat-card-subtitle>
          </mat-card-header>
          
          <mat-card-content>
            <div class="xp-thresholds" [formGroup]="evolutionForm">
              <div formGroupName="xpThresholds">
              <div class="thresholds-grid">
                <mat-form-field appearance="outline" *ngFor="let piece of pieceTypes" class="threshold-field">
                  <mat-label>{{ piece.name }} XP</mat-label>
                  <input matInput 
                         type="number" 
                         [formControlName]="piece.key"
                         [placeholder]="getDefaultXp(piece.key)"
                         min="0">
                  <mat-icon matSuffix>trending_up</mat-icon>
                </mat-form-field>
              </div>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Evolution Rules -->
        <mat-card class="evolution-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>account_tree</mat-icon>
            <mat-card-title>Evolution Rules</mat-card-title>
            <mat-card-subtitle>Define which pieces each type can evolve into</mat-card-subtitle>
          </mat-card-header>
          
          <mat-card-content>
            <div class="evolution-rules" [formGroup]="evolutionForm">
              <div formGroupName="rules">
              <div class="rules-list">
                <div *ngFor="let piece of pieceTypes" class="rule-item">
                  <div class="rule-header">
                    <mat-icon>{{ piece.icon }}</mat-icon>
                    <span class="piece-name">{{ piece.name }}</span>
                    <span class="rule-description">{{ piece.description }}</span>
                  </div>
                  
                  <div class="rule-targets">
                    <mat-form-field appearance="outline" class="targets-field">
                      <mat-label>Can evolve into</mat-label>
                      <mat-select [formControlName]="piece.key" multiple>
                        <mat-option *ngFor="let target of getAvailableTargets(piece.key)" 
                                    [value]="target.key">
                          <mat-icon>{{ target.icon }}</mat-icon>
                          {{ target.name }}
                        </mat-option>
                      </mat-select>
                    </mat-form-field>
                    
                    <div class="selected-targets">
                      <mat-chip *ngFor="let target of getSelectedTargets(piece.key)" 
                                [removable]="true" 
                                (removed)="removeTarget(piece.key, target)">
                        <mat-icon matChipAvatar>{{ target.icon }}</mat-icon>
                        {{ target.name }}
                        <mat-icon matChipTrailingIcon>cancel</mat-icon>
                      </mat-chip>
                    </div>
                  </div>
                </div>
              </div>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Immediate Evolution on Last Rank -->
        <mat-card class="evolution-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>speed</mat-icon>
            <mat-card-title>Immediate Evolution</mat-card-title>
            <mat-card-subtitle>Pieces that evolve immediately when reaching the last rank</mat-card-subtitle>
          </mat-card-header>
          
          <mat-card-content>
            <div class="immediate-evolution" [formGroup]="evolutionForm">
              <div formGroupName="immediateOnLastRank">
              <div class="immediate-grid">
                <div *ngFor="let piece of pieceTypes" class="immediate-item">
                  <mat-checkbox [formControlName]="piece.key">
                    <mat-icon>{{ piece.icon }}</mat-icon>
                    {{ piece.name }}
                  </mat-checkbox>
                </div>
            </div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
    </div>
  `,
  styles: [`
    .evolution-form {
      padding: 1rem;
    }

    .form-header {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-bottom: 1.5rem;
      padding: 1rem;
      background: linear-gradient(135deg, #4ecdc4 0%, #44a08d 100%);
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

    .evolution-sections {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .evolution-card {
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

    .thresholds-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1rem;
    }

    .threshold-field {
      width: 100%;
    }

    .rules-list {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .rule-item {
      padding: 1rem;
      border: 1px solid #e9ecef;
      border-radius: 8px;
      background: #f8f9fa;
    }

    .rule-header {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-bottom: 1rem;
    }

    .piece-name {
      font-weight: 600;
      color: #495057;
      font-size: 1.1rem;
    }

    .rule-description {
      color: #6c757d;
      font-size: 0.9rem;
      margin-left: auto;
    }

    .rule-targets {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .targets-field {
      width: 100%;
    }

    .selected-targets {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      min-height: 2rem;
      padding: 0.5rem;
      border: 1px dashed #dee2e6;
      border-radius: 4px;
      background: white;
    }

    .immediate-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1rem;
    }

    .immediate-item {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem;
      border: 1px solid #e9ecef;
      border-radius: 4px;
      background: white;
    }

    .mat-mdc-form-field {
      width: 100%;
    }

    .mat-mdc-chip {
      margin: 0.25rem;
    }

    .mat-mdc-checkbox {
      width: 100%;
    }

    @media (max-width: 768px) {
      .thresholds-grid,
      .immediate-grid {
        grid-template-columns: 1fr;
      }
      
      .rule-header {
        flex-direction: column;
        align-items: flex-start;
      }
      
      .rule-description {
        margin-left: 0;
        margin-top: 0.5rem;
      }
    }
  `]
})
export class EvolutionFormComponent {
  @Input() evolutionForm!: FormGroup;

  readonly pieceTypes: PieceEvolution[] = [
    { key: 'Pawn', name: 'Pawn', icon: 'person', description: 'Basic unit with evolution potential' },
    { key: 'Knight', name: 'Knight', icon: 'directions_horse', description: 'Mobile cavalry unit' },
    { key: 'Bishop', name: 'Bishop', icon: 'church', description: 'Ranged support unit' },
    { key: 'Rook', name: 'Rook', icon: 'home', description: 'Heavy defensive unit' },
    { key: 'Queen', name: 'Queen', icon: 'star', description: 'Most powerful piece' },
    { key: 'King', name: 'King', icon: 'crown', description: 'Royal piece' }
  ];

  getDefaultXp(pieceKey: string): string {
    const defaults: Record<string, string> = {
      'Pawn': '20',
      'Knight': '40',
      'Bishop': '40',
      'Rook': '60',
      'Queen': '0',
      'King': '0'
    };
    return defaults[pieceKey] || '0';
  }

  getAvailableTargets(pieceKey: string): PieceEvolution[] {
    // Define evolution rules - which pieces can evolve into what
    const evolutionRules: Record<string, string[]> = {
      'Pawn': ['Knight', 'Bishop'],
      'Knight': ['Rook'],
      'Bishop': ['Rook'],
      'Rook': ['Queen'],
      'Queen': [],
      'King': []
    };

    const allowedTargets = evolutionRules[pieceKey] || [];
    return this.pieceTypes.filter(piece => allowedTargets.includes(piece.key));
  }

  getSelectedTargets(pieceKey: string): PieceEvolution[] {
    const selectedValues = this.evolutionForm.get(`rules.${pieceKey}`)?.value || [];
    return this.pieceTypes.filter(piece => selectedValues.includes(piece.key));
  }

  removeTarget(pieceKey: string, target: PieceEvolution): void {
    const currentValues = this.evolutionForm.get(`rules.${pieceKey}`)?.value || [];
    const newValues = currentValues.filter((value: string) => value !== target.key);
    this.evolutionForm.get(`rules.${pieceKey}`)?.setValue(newValues);
  }
}
