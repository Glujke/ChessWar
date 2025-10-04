import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-ai-form',
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
    <div class="ai-form">
      <div class="form-header">
        <mat-icon>psychology</mat-icon>
        <h2>Настройки ИИ</h2>
        <p>Настройте поведение искусственного интеллекта</p>
      </div>

      <mat-card class="ai-card">
        <mat-card-header>
          <mat-icon mat-card-avatar>smart_toy</mat-icon>
          <mat-card-title>Поведение ИИ</mat-card-title>
          <mat-card-subtitle>Настройте, как ИИ принимает решения</mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <div class="ai-settings" [formGroup]="aiForm">
            <mat-form-field appearance="outline" class="ai-field">
              <mat-label>Порог XP для эволюции</mat-label>
              <input matInput 
                     type="number" 
                     formControlName="nearEvolutionXp"
                     placeholder="19"
                     min="0">
              <mat-icon matSuffix>trending_up</mat-icon>
              <mat-hint>Порог XP, при котором ИИ считает эволюцию приоритетной</mat-hint>
            </mat-form-field>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card class="last-rank-card">
        <mat-card-header>
          <mat-icon mat-card-avatar>border_all</mat-icon>
          <mat-card-title>Last Rank Edge Y</mat-card-title>
          <mat-card-subtitle>Y-coordinate for last rank for each race</mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <div class="last-rank-settings" [formGroup]="aiForm">
            <div formGroupName="lastRankEdgeY">
            <div class="race-grid">
              <mat-form-field appearance="outline" class="race-field">
                <mat-label>Elves Last Rank</mat-label>
                <input matInput 
                       type="number" 
                       formControlName="Elves"
                       placeholder="6"
                       min="0"
                       max="7">
                <mat-icon matSuffix>nature</mat-icon>
                <mat-hint>Y-coordinate for Elves last rank (0-7)</mat-hint>
              </mat-form-field>

              <mat-form-field appearance="outline" class="race-field">
                <mat-label>Orcs Last Rank</mat-label>
                <input matInput 
                       type="number" 
                       formControlName="Orcs"
                       placeholder="1"
                       min="0"
                       max="7">
                <mat-icon matSuffix>sports_mma</mat-icon>
                <mat-hint>Y-coordinate for Orcs last rank (0-7)</mat-hint>
              </mat-form-field>
            </div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card class="king-aura-card">
        <mat-card-header>
          <mat-icon mat-card-avatar>auto_awesome</mat-icon>
          <mat-card-title>King Aura</mat-card-title>
          <mat-card-subtitle>King's aura effect on nearby units</mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <div class="king-aura-settings" [formGroup]="aiForm">
            <div formGroupName="kingAura">
            <div class="aura-grid">
              <mat-form-field appearance="outline" class="aura-field">
                <mat-label>Aura Radius</mat-label>
                <input matInput 
                       type="number" 
                       formControlName="radius"
                       placeholder="3"
                       min="0">
                <mat-icon matSuffix>gps_fixed</mat-icon>
                <mat-hint>Radius of king's aura effect</mat-hint>
              </mat-form-field>

              <mat-form-field appearance="outline" class="aura-field">
                <mat-label>Attack Bonus</mat-label>
                <input matInput 
                       type="number" 
                       formControlName="atkBonus"
                       placeholder="1"
                       min="0">
                <mat-icon matSuffix>bolt</mat-icon>
                <mat-hint>Attack bonus for units in aura</mat-hint>
              </mat-form-field>
            </div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .ai-form {
      padding: 1rem;
    }

    .form-header {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-bottom: 1.5rem;
      padding: 1rem;
      background: linear-gradient(135deg, #a8edea 0%, #fed6e3 100%);
      border-radius: 8px;
      color: #2c3e50;
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

    .ai-card,
    .last-rank-card,
    .king-aura-card {
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

    .ai-settings {
      padding: 1rem 0;
    }

    .ai-field {
      width: 100%;
      max-width: 400px;
    }

    .race-grid,
    .aura-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 1rem;
    }

    .race-field,
    .aura-field {
      width: 100%;
    }

    .mat-mdc-form-field {
      font-size: 0.9rem;
    }

    @media (max-width: 768px) {
      .race-grid,
      .aura-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class AiFormComponent {
  @Input() aiForm!: FormGroup;
}
