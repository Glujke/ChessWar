import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-globals-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatSelectModule
  ],
  template: `
    <div class="globals-form">
      <div class="form-header">
        <mat-icon>settings</mat-icon>
        <h2>Глобальные настройки</h2>
        <p>Настройте глобальные параметры игры</p>
      </div>

      <mat-card class="globals-card">
        <mat-card-content>
          <div class="globals-fields" [formGroup]="globalsForm">
            <mat-form-field appearance="outline" class="global-field">
              <mat-label>Восстановление MP за ход</mat-label>
              <input matInput 
                     type="number" 
                     formControlName="mpRegenPerTurn"
                     placeholder="10"
                     min="0">
              <mat-icon matSuffix>flash_on</mat-icon>
              <mat-hint>Очки маны, восстанавливаемые каждый ход</mat-hint>
            </mat-form-field>

            <mat-form-field appearance="outline" class="global-field">
              <mat-label>Фаза уменьшения перезарядки</mat-label>
              <mat-select formControlName="cooldownTickPhase">
                <mat-option value="EndTurn">Конец хода</mat-option>
                <mat-option value="StartTurn">Начало хода</mat-option>
              </mat-select>
              <mat-icon matSuffix>schedule</mat-icon>
              <mat-hint>Когда уменьшается перезарядка способностей</mat-hint>
            </mat-form-field>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .globals-form {
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

    .globals-card {
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    }

    .globals-fields {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 1.5rem;
      padding: 1rem 0;
    }

    .global-field {
      width: 100%;
    }

    .mat-mdc-form-field {
      font-size: 1rem;
    }

    @media (max-width: 768px) {
      .globals-fields {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class GlobalsFormComponent {
  @Input() globalsForm!: FormGroup;
}
