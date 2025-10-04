import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, FormArray, FormBuilder, ReactiveFormsModule, Validators, AbstractControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';

interface AbilityTemplate {
  key: string;
  name: string;
  piece: string;
  description: string;
  icon: string;
}

@Component({
  selector: 'app-abilities-form',
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
    MatExpansionModule
  ],
  template: `
    <div class="abilities-form">
      <div class="form-header">
        <mat-icon>auto_awesome</mat-icon>
        <h2>Настройка способностей</h2>
        <p>Настройте специальные способности для каждого типа фигур</p>
      </div>

      <div class="abilities-controls">
        <button mat-raised-button color="primary" (click)="addAbility()">
          <mat-icon>add</mat-icon>
          Добавить способность
        </button>
        <button mat-button (click)="loadTemplates()">
          <mat-icon>refresh</mat-icon>
          Загрузить шаблоны
        </button>
      </div>

      <div class="abilities-list">
        <mat-expansion-panel *ngFor="let ability of abilitiesArray.controls; let i = index" 
                             [expanded]="i === 0"
                             class="ability-panel">
          <mat-expansion-panel-header>
            <mat-panel-title>
              <mat-icon>{{ getAbilityIcon(ability.get('name')?.value) }}</mat-icon>
              {{ ability.get('name')?.value || 'Новая способность' }}
            </mat-panel-title>
            <mat-panel-description>
              {{ ability.get('piece')?.value }} - {{ ability.get('description')?.value || 'Без описания' }}
            </mat-panel-description>
          </mat-expansion-panel-header>

          <div class="ability-form" [formGroup]="getAbilityFormGroup(ability)!" *ngIf="isFormGroup(ability)">
            <div class="ability-basic-info">
              <mat-form-field appearance="outline" class="ability-field">
                <mat-label>Название способности</mat-label>
                <input matInput 
                       formControlName="name"
                       placeholder="например, LightArrow"
                       (blur)="updateAbilityKey(ability)">
                <mat-icon matSuffix>label</mat-icon>
              </mat-form-field>

              <mat-form-field appearance="outline" class="ability-field">
                <mat-label>Тип фигуры</mat-label>
                <mat-select formControlName="piece">
                  <mat-option value="Pawn">Pawn</mat-option>
                  <mat-option value="Knight">Knight</mat-option>
                  <mat-option value="Bishop">Bishop</mat-option>
                  <mat-option value="Rook">Rook</mat-option>
                  <mat-option value="Queen">Queen</mat-option>
                  <mat-option value="King">King</mat-option>
                </mat-select>
                <mat-icon matSuffix>extension</mat-icon>
              </mat-form-field>

              <mat-form-field appearance="outline" class="ability-field full-width">
                <mat-label>Описание</mat-label>
                <input matInput 
                       formControlName="description"
                       placeholder="Краткое описание способности">
                <mat-icon matSuffix>description</mat-icon>
              </mat-form-field>
            </div>

            <mat-divider></mat-divider>

            <div class="ability-stats">
              <h4>Параметры способности</h4>
              <div class="stats-grid">
                <mat-form-field appearance="outline" class="stat-field">
                  <mat-label>Стоимость MP</mat-label>
                  <input matInput 
                         type="number" 
                         formControlName="mpCost"
                         placeholder="3"
                         min="0">
                  <mat-icon matSuffix>flash_on</mat-icon>
                </mat-form-field>

                <mat-form-field appearance="outline" class="stat-field">
                  <mat-label>Перезарядка (ходы)</mat-label>
                  <input matInput 
                         type="number" 
                         formControlName="cooldown"
                         placeholder="2"
                         min="0">
                  <mat-icon matSuffix>schedule</mat-icon>
                </mat-form-field>

                <mat-form-field appearance="outline" class="stat-field">
                  <mat-label>Дальность</mat-label>
                  <input matInput 
                         type="number" 
                         formControlName="range"
                         placeholder="4"
                         min="0">
                  <mat-icon matSuffix>gps_fixed</mat-icon>
                </mat-form-field>

                <div class="checkbox-field">
                  <mat-checkbox formControlName="isAoe">
                    Область действия
                  </mat-checkbox>
                </div>
              </div>

              <div class="ability-effects">
                <h5>Эффекты</h5>
                <div class="effects-grid">
                  <mat-form-field appearance="outline" class="effect-field">
                    <mat-label>Урон</mat-label>
                    <input matInput 
                           type="number" 
                           formControlName="damage"
                           placeholder="4"
                           min="0">
                    <mat-icon matSuffix>bolt</mat-icon>
                  </mat-form-field>

                  <mat-form-field appearance="outline" class="effect-field">
                    <mat-label>Лечение</mat-label>
                    <input matInput 
                           type="number" 
                           formControlName="heal"
                           placeholder="5"
                           min="0">
                    <mat-icon matSuffix>healing</mat-icon>
                  </mat-form-field>

                  <mat-form-field appearance="outline" class="effect-field">
                    <mat-label>Количество ударов</mat-label>
                    <input matInput 
                           type="number" 
                           formControlName="hits"
                           placeholder="2"
                           min="0">
                    <mat-icon matSuffix>repeat</mat-icon>
                  </mat-form-field>

                  <mat-form-field appearance="outline" class="effect-field">
                    <mat-label>Damage per Hit</mat-label>
                    <input matInput 
                           type="number" 
                           formControlName="damagePerHit"
                           placeholder="3"
                           min="0">
                    <mat-icon matSuffix>bolt</mat-icon>
                  </mat-form-field>

                  <mat-form-field appearance="outline" class="effect-field">
                    <mat-label>Temp HP Multiplier</mat-label>
                    <input matInput 
                           type="number" 
                           formControlName="tempHpMultiplier"
                           placeholder="2"
                           min="0">
                    <mat-icon matSuffix>shield</mat-icon>
                  </mat-form-field>

                  <mat-form-field appearance="outline" class="effect-field">
                    <mat-label>Duration (turns)</mat-label>
                    <input matInput 
                           type="number" 
                           formControlName="durationTurns"
                           placeholder="1"
                           min="0">
                    <mat-icon matSuffix>schedule</mat-icon>
                  </mat-form-field>
                </div>

                <div class="ability-options">
                  <mat-checkbox formControlName="grantsExtraTurn">
                    Grants Extra Turn
                  </mat-checkbox>
                  <mat-checkbox formControlName="diagonalOnly">
                    Diagonal Only
                  </mat-checkbox>
                </div>
              </div>
            </div>

            <div class="ability-actions">
              <button mat-button color="warn" (click)="removeAbility(i)">
                <mat-icon>delete</mat-icon>
                Удалить
              </button>
            </div>
          </div>
        </mat-expansion-panel>
      </div>
    </div>
  `,
  styles: [`
    .abilities-form {
      padding: 1rem;
    }

    .form-header {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-bottom: 1.5rem;
      padding: 1rem;
      background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%);
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

    .abilities-controls {
      display: flex;
      gap: 1rem;
      margin-bottom: 1.5rem;
    }

    .abilities-list {
      margin-top: 1rem;
    }

    .ability-panel {
      margin-bottom: 1rem;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .ability-form {
      padding: 1rem;
    }

    .ability-basic-info {
      display: grid;
      grid-template-columns: 1fr 1fr 2fr;
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .full-width {
      grid-column: 1 / -1;
    }

    .ability-stats h4 {
      margin: 1rem 0 0.5rem 0;
      color: #495057;
      font-size: 1.1rem;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .checkbox-field {
      display: flex;
      align-items: center;
      padding: 0.5rem 0;
    }

    .ability-effects h5 {
      margin: 1rem 0 0.5rem 0;
      color: #6c757d;
      font-size: 1rem;
    }

    .effects-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .ability-options {
      display: flex;
      gap: 2rem;
      margin-top: 1rem;
    }

    .ability-actions {
      display: flex;
      justify-content: flex-end;
      margin-top: 1rem;
      padding-top: 1rem;
      border-top: 1px solid #e9ecef;
    }

    .mat-mdc-form-field {
      width: 100%;
    }

    .mat-mdc-expansion-panel-header {
      background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
    }

    .mat-mdc-expansion-panel-header-title {
      color: #495057;
      font-weight: 600;
    }

    .mat-mdc-expansion-panel-header-description {
      color: #6c757d;
    }

    @media (max-width: 768px) {
      .ability-basic-info {
        grid-template-columns: 1fr;
      }
      
      .stats-grid,
      .effects-grid {
        grid-template-columns: 1fr;
      }
      
      .ability-options {
        flex-direction: column;
        gap: 0.5rem;
      }
    }
  `]
})
export class AbilitiesFormComponent {
  @Input() abilitiesForm!: FormGroup;
  private readonly fb = inject(FormBuilder);

  readonly abilityTemplates: AbilityTemplate[] = [
    { key: 'Bishop.LightArrow', name: 'LightArrow', piece: 'Bishop', description: 'Ranged attack with light damage', icon: 'bolt' },
    { key: 'Bishop.Heal', name: 'Heal', piece: 'Bishop', description: 'Heal nearby ally', icon: 'healing' },
    { key: 'Knight.DoubleStrike', name: 'DoubleStrike', piece: 'Knight', description: 'Attack twice in one turn', icon: 'repeat' },
    { key: 'Rook.Fortress', name: 'Fortress', piece: 'Rook', description: 'Gain temporary HP boost', icon: 'shield' },
    { key: 'Queen.MagicExplosion', name: 'MagicExplosion', piece: 'Queen', description: 'Area damage spell', icon: 'explosion' },
    { key: 'Queen.Resurrection', name: 'Resurrection', piece: 'Queen', description: 'Revive fallen ally', icon: 'refresh' },
    { key: 'King.RoyalCommand', name: 'RoyalCommand', piece: 'King', description: 'Command nearby units', icon: 'crown' },
    { key: 'Pawn.ShieldBash', name: 'ShieldBash', piece: 'Pawn', description: 'Basic shield attack', icon: 'shield' },
    { key: 'Pawn.Breakthrough', name: 'Breakthrough', piece: 'Pawn', description: 'Charge attack', icon: 'directions_run' }
  ];

  get abilitiesArray(): FormArray {
    return this.abilitiesForm.get('abilities') as FormArray;
  }

  addAbility(): void {
    const abilityGroup = this.fb.group({
      name: ['', [Validators.required]],
      piece: ['Pawn', [Validators.required]],
      description: [''],
      mpCost: [0, [Validators.required, Validators.min(0)]],
      cooldown: [0, [Validators.required, Validators.min(0)]],
      range: [0, [Validators.required, Validators.min(0)]],
      isAoe: [false],
      damage: [0, [Validators.min(0)]],
      heal: [0, [Validators.min(0)]],
      hits: [0, [Validators.min(0)]],
      damagePerHit: [0, [Validators.min(0)]],
      tempHpMultiplier: [0, [Validators.min(0)]],
      durationTurns: [0, [Validators.min(0)]],
      grantsExtraTurn: [false],
      diagonalOnly: [false]
    });

    this.abilitiesArray.push(abilityGroup);
  }

  removeAbility(index: number): void {
    this.abilitiesArray.removeAt(index);
  }

  loadTemplates(): void {
    // Clear existing abilities
    while (this.abilitiesArray.length !== 0) {
      this.abilitiesArray.removeAt(0);
    }

    // Add template abilities
    this.abilityTemplates.forEach(template => {
      const abilityGroup = this.fb.group({
        name: [template.name, [Validators.required]],
        piece: [template.piece, [Validators.required]],
        description: [template.description],
        mpCost: [3, [Validators.required, Validators.min(0)]],
        cooldown: [2, [Validators.required, Validators.min(0)]],
        range: [1, [Validators.required, Validators.min(0)]],
        isAoe: [false],
        damage: [0, [Validators.min(0)]],
        heal: [0, [Validators.min(0)]],
        hits: [0, [Validators.min(0)]],
        damagePerHit: [0, [Validators.min(0)]],
        tempHpMultiplier: [0, [Validators.min(0)]],
        durationTurns: [0, [Validators.min(0)]],
        grantsExtraTurn: [false],
        diagonalOnly: [false]
      });

      // Set specific values based on template
      this.setTemplateValues(abilityGroup, template.key);

      this.abilitiesArray.push(abilityGroup);
    });
  }

  private setTemplateValues(abilityGroup: FormGroup, templateKey: string): void {
    const templates: Record<string, any> = {
      'Bishop.LightArrow': { mpCost: 3, cooldown: 2, range: 4, damage: 4 },
      'Bishop.Heal': { mpCost: 6, cooldown: 4, range: 2, heal: 5 },
      'Knight.DoubleStrike': { mpCost: 5, cooldown: 3, range: 1, hits: 2, damagePerHit: 3 },
      'Rook.Fortress': { mpCost: 8, cooldown: 5, range: 0, tempHpMultiplier: 2, durationTurns: 1 },
      'Queen.MagicExplosion': { mpCost: 10, cooldown: 3, range: 3, isAoe: true, damage: 7 },
      'Queen.Resurrection': { mpCost: 12, cooldown: 10, range: 3, heal: 20 },
      'King.RoyalCommand': { mpCost: 10, cooldown: 6, range: 8 },
      'Pawn.ShieldBash': { mpCost: 2, cooldown: 2, range: 1, damage: 2 },
      'Pawn.Breakthrough': { mpCost: 2, cooldown: 2, range: 1, damage: 3 }
    };

    const template = templates[templateKey];
    if (template) {
      Object.keys(template).forEach(key => {
        abilityGroup.get(key)?.setValue(template[key]);
      });
    }
  }

  updateAbilityKey(ability: AbstractControl): void {
    const name = ability.get('name')?.value;
    const piece = ability.get('piece')?.value;
    if (name && piece) {
      // Update the key in the parent form if needed
      console.log(`Ability key would be: ${piece}.${name}`);
    }
  }

  getAbilityIcon(abilityName: string): string {
    const iconMap: Record<string, string> = {
      'LightArrow': 'bolt',
      'Heal': 'healing',
      'DoubleStrike': 'repeat',
      'Fortress': 'shield',
      'MagicExplosion': 'explosion',
      'Resurrection': 'refresh',
      'RoyalCommand': 'crown',
      'ShieldBash': 'shield',
      'Breakthrough': 'directions_run'
    };
    return iconMap[abilityName] || 'auto_awesome';
  }

  getAbilityFormGroup(ability: AbstractControl): FormGroup | null {
    return ability instanceof FormGroup ? ability : null;
  }

  isFormGroup(control: AbstractControl): boolean {
    return control instanceof FormGroup;
  }

}
