import { Injectable, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { ConfigApiService } from '../services/config-api.service';
import { BalanceConfig, BalanceVersion } from '../models/balance-config.model';

export interface FormSection {
  key: string;
  title: string;
  icon: string;
  component: string;
}

@Injectable()
export class ConfigEditorViewModel {
  private readonly fb = inject(FormBuilder);
  private readonly configApi = inject(ConfigApiService);

  // Signals for reactive state
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly errors = signal<string[]>([]);
  readonly versionInfo = signal<BalanceVersion | null>(null);
  readonly isEditMode = signal(false);

  // Main form group
  readonly configForm = this.fb.group({
    version: ['1.0.0', [Validators.required]],
    globals: this.fb.group({
      mpRegenPerTurn: [10, [Validators.required, Validators.min(0)]],
      cooldownTickPhase: ['EndTurn', [Validators.required]]
    }),
    playerMana: this.fb.group({
      initialMana: [10, [Validators.required, Validators.min(0)]],
      maxMana: [50, [Validators.required, Validators.min(1)]],
      manaRegenPerTurn: [10, [Validators.required, Validators.min(0)]],
      mandatoryAction: [true, [Validators.required]],
      attackCost: [1, [Validators.required, Validators.min(0)]],
      movementCosts: this.fb.group({
        Pawn: [1, [Validators.required, Validators.min(0)]],
        Knight: [2, [Validators.required, Validators.min(0)]],
        Bishop: [3, [Validators.required, Validators.min(0)]],
        Rook: [3, [Validators.required, Validators.min(0)]],
        Queen: [4, [Validators.required, Validators.min(0)]],
        King: [4, [Validators.required, Validators.min(0)]]
      })
    }),
    pieces: this.fb.group({
      Pawn: this.createPieceFormGroup(),
      Knight: this.createPieceFormGroup(),
      Bishop: this.createPieceFormGroup(),
      Rook: this.createPieceFormGroup(),
      Queen: this.createPieceFormGroup(),
      King: this.createPieceFormGroup()
    }),
    abilities: this.fb.group({
      abilities: this.fb.array([])
    }),
    evolution: this.fb.group({
      xpThresholds: this.fb.group({
        Pawn: [20, [Validators.required, Validators.min(0)]],
        Knight: [40, [Validators.required, Validators.min(0)]],
        Bishop: [40, [Validators.required, Validators.min(0)]],
        Rook: [60, [Validators.required, Validators.min(0)]],
        Queen: [0, [Validators.required, Validators.min(0)]],
        King: [0, [Validators.required, Validators.min(0)]]
      }),
      rules: this.fb.group({
        Pawn: this.fb.array(['Knight', 'Bishop']),
        Knight: this.fb.array(['Rook']),
        Bishop: this.fb.array(['Rook']),
        Rook: this.fb.array(['Queen']),
        Queen: this.fb.array([]),
        King: this.fb.array([])
      }),
      immediateOnLastRank: this.fb.group({
        Pawn: [true],
        Knight: [false],
        Bishop: [false],
        Rook: [false],
        Queen: [false],
        King: [false]
      })
    }),
    ai: this.fb.group({
      nearEvolutionXp: [19, [Validators.required, Validators.min(0)]],
      lastRankEdgeY: this.fb.group({
        Elves: [6, [Validators.required, Validators.min(0), Validators.max(7)]],
        Orcs: [1, [Validators.required, Validators.min(0), Validators.max(7)]]
      }),
      kingAura: this.fb.group({
        radius: [3, [Validators.required, Validators.min(0)]],
        atkBonus: [1, [Validators.required, Validators.min(0)]]
      })
    }),
    shieldSystem: this.fb.group({
      king: this.fb.group({
        baseRegen: [5, [Validators.required, Validators.min(0)]],
        proximityBonus1: this.fb.group({
          pawn: [1, [Validators.required, Validators.min(0)]],
          knight: [2, [Validators.required, Validators.min(0)]],
          bishop: [2, [Validators.required, Validators.min(0)]],
          rook: [3, [Validators.required, Validators.min(0)]],
          queen: [4, [Validators.required, Validators.min(0)]],
          king: [0, [Validators.required, Validators.min(0)]]
        }),
        proximityBonus2: this.fb.group({
          pawn: [0, [Validators.required, Validators.min(0)]],
          knight: [1, [Validators.required, Validators.min(0)]],
          bishop: [1, [Validators.required, Validators.min(0)]],
          rook: [2, [Validators.required, Validators.min(0)]],
          queen: [3, [Validators.required, Validators.min(0)]],
          king: [0, [Validators.required, Validators.min(0)]]
        })
      }),
      ally: this.fb.group({
        neighborContribution: this.fb.group({
          pawn: [1, [Validators.required, Validators.min(0)]],
          knight: [2, [Validators.required, Validators.min(0)]],
          bishop: [2, [Validators.required, Validators.min(0)]],
          rook: [3, [Validators.required, Validators.min(0)]],
          queen: [4, [Validators.required, Validators.min(0)]],
          king: [5, [Validators.required, Validators.min(0)]]
        })
      })
    }),
    killRewards: this.fb.group({
      pawn: [10, [Validators.required, Validators.min(0)]],
      knight: [20, [Validators.required, Validators.min(0)]],
      bishop: [20, [Validators.required, Validators.min(0)]],
      rook: [30, [Validators.required, Validators.min(0)]],
      queen: [50, [Validators.required, Validators.min(0)]],
      king: [100, [Validators.required, Validators.min(0)]]
    })
  });

  // Form sections for navigation
  readonly formSections: FormSection[] = [
    { key: 'globals', title: 'Globals', icon: 'settings', component: 'globals-form' },
    { key: 'playerMana', title: 'Player Mana', icon: 'flash_on', component: 'player-mana-form' },
    { key: 'pieces', title: 'Pieces', icon: 'extension', component: 'pieces-form' },
    { key: 'abilities', title: 'Abilities', icon: 'auto_awesome', component: 'abilities-form' },
    { key: 'evolution', title: 'Evolution', icon: 'trending_up', component: 'evolution-form' },
    { key: 'ai', title: 'AI Settings', icon: 'psychology', component: 'ai-form' },
    { key: 'shieldSystem', title: 'Shield System', icon: 'shield', component: 'shield-form' },
    { key: 'killRewards', title: 'Kill Rewards', icon: 'emoji_events', component: 'kill-rewards-form' }
  ];

  private createPieceFormGroup(): FormGroup {
    return this.fb.group({
      hp: [10, [Validators.required, Validators.min(1)]],
      atk: [2, [Validators.required, Validators.min(0)]],
      range: [1, [Validators.required, Validators.min(0)]],
      movement: [1, [Validators.required, Validators.min(0)]],
      xpToEvolve: [20, [Validators.required, Validators.min(0)]],
      maxShieldHP: [50, [Validators.required, Validators.min(0)]]
    });
  }

  // Load configuration from API
  async loadConfig(versionId: string, versionInfo?: BalanceVersion): Promise<void> {
    this.loading.set(true);
    this.errors.set([]);

    try {
      if (versionInfo) {
        this.versionInfo.set(versionInfo);
      }

      const json = await this.configApi.getPayload(versionId).toPromise();
      if (json) {
        const config = JSON.parse(json);
        
        // Handle abilities specially - convert from object to array format
        if (config.abilities && typeof config.abilities === 'object' && !Array.isArray(config.abilities)) {
          const abilitiesArray = Object.keys(config.abilities).map(key => {
            const ability = config.abilities[key];
            return {
              name: key.split('.')[1] || key,
              piece: key.split('.')[0] || 'Pawn',
              description: ability.description || '',
              ...ability
            };
          });
          config.abilities = { abilities: abilitiesArray };
        }
        
        this.configForm.patchValue(config);
        
        // Initialize abilities FormArray if we have abilities data
        if (config.abilities && config.abilities.abilities && Array.isArray(config.abilities.abilities)) {
          const abilitiesArray = this.configForm.get('abilities.abilities') as FormArray;
          if (abilitiesArray) {
            abilitiesArray.clear();
            config.abilities.abilities.forEach((ability: any) => {
              const abilityGroup = this.fb.group({
                name: [ability.name || '', [Validators.required]],
                piece: [ability.piece || 'Pawn', [Validators.required]],
                description: [ability.description || ''],
                mpCost: [ability.mpCost || 0, [Validators.required, Validators.min(0)]],
                cooldown: [ability.cooldown || 0, [Validators.required, Validators.min(0)]],
                range: [ability.range || 0, [Validators.required, Validators.min(0)]],
                isAoe: [ability.isAoe || false],
                damage: [ability.damage || 0, [Validators.min(0)]],
                heal: [ability.heal || 0, [Validators.min(0)]],
                hits: [ability.hits || 0, [Validators.min(0)]],
                damagePerHit: [ability.damagePerHit || 0, [Validators.min(0)]],
                tempHpMultiplier: [ability.tempHpMultiplier || 0, [Validators.min(0)]],
                durationTurns: [ability.durationTurns || 0, [Validators.min(0)]],
                grantsExtraTurn: [ability.grantsExtraTurn || false],
                diagonalOnly: [ability.diagonalOnly || false]
              });
              abilitiesArray.push(abilityGroup);
            });
          }
        }
      }
    } catch (error: any) {
      if (error.status === 404) {
        // Load default template
        this.loadDefaultConfig();
      } else {
        this.errors.set([`Не удалось загрузить конфигурацию: ${error.message}`]);
      }
    } finally {
      this.loading.set(false);
    }
  }

  // Save configuration to API
  async saveConfig(versionId: string): Promise<void> {
    if (this.configForm.invalid) {
      this.markFormGroupTouched(this.configForm);
      return;
    }

    this.saving.set(true);
    this.errors.set([]);

    try {
      const formValue = this.configForm.value;
      
      // Handle abilities specially - convert from array to object format
      if (formValue.abilities && formValue.abilities.abilities && Array.isArray(formValue.abilities.abilities)) {
        const abilitiesObject: Record<string, any> = {};
        formValue.abilities.abilities.forEach((ability: any) => {
          if (ability.name && ability.piece) {
            const key = `${ability.piece}.${ability.name}`;
            const { name, piece, description, ...abilityData } = ability;
            abilitiesObject[key] = abilityData;
          }
        });
        formValue.abilities = abilitiesObject;
      }
      
      const json = JSON.stringify(formValue, null, 2);
      await this.configApi.savePayload(versionId, json).toPromise();
    } catch (error: any) {
      this.errors.set([`Не удалось сохранить конфигурацию: ${error.message}`]);
    } finally {
      this.saving.set(false);
    }
  }

  // Publish configuration
  async publishConfig(versionId: string): Promise<void> {
    this.saving.set(true);
    this.errors.set([]);

    try {
      await this.configApi.publishVersion(versionId).toPromise();
    } catch (error: any) {
      this.errors.set([`Не удалось опубликовать конфигурацию: ${error.message}`]);
    } finally {
      this.saving.set(false);
    }
  }

  // Load default configuration
  private loadDefaultConfig(): void {
    const defaultConfig = {
      version: "1.0.0",
      globals: {
        mpRegenPerTurn: 10,
        cooldownTickPhase: "EndTurn"
      },
      playerMana: {
        initialMana: 10,
        maxMana: 50,
        manaRegenPerTurn: 10,
        mandatoryAction: true,
        attackCost: 1,
        movementCosts: {
          Pawn: 1,
          Knight: 2,
          Bishop: 3,
          Rook: 3,
          Queen: 4,
          King: 4
        }
      },
      pieces: {
        Pawn: { hp: 10, atk: 2, range: 1, movement: 1, xpToEvolve: 20, maxShieldHP: 50 },
        Knight: { hp: 20, atk: 4, range: 1, movement: 1, xpToEvolve: 40, maxShieldHP: 80 },
        Bishop: { hp: 18, atk: 3, range: 4, movement: 8, xpToEvolve: 40, maxShieldHP: 80 },
        Rook: { hp: 25, atk: 5, range: 8, movement: 8, xpToEvolve: 60, maxShieldHP: 100 },
        Queen: { hp: 30, atk: 7, range: 3, movement: 8, xpToEvolve: 0, maxShieldHP: 150 },
        King: { hp: 50, atk: 3, range: 1, movement: 1, xpToEvolve: 0, maxShieldHP: 400 }
      },
      abilities: {
        abilities: [
          {
            name: 'LightArrow',
            piece: 'Bishop',
            description: 'Ranged attack with light damage',
            mpCost: 3,
            cooldown: 2,
            range: 4,
            isAoe: false,
            damage: 4
          },
          {
            name: 'Heal',
            piece: 'Bishop',
            description: 'Heal nearby ally',
            mpCost: 6,
            cooldown: 4,
            range: 2,
            isAoe: false,
            heal: 5
          },
          {
            name: 'DoubleStrike',
            piece: 'Knight',
            description: 'Attack twice in one turn',
            mpCost: 5,
            cooldown: 3,
            range: 1,
            isAoe: false,
            hits: 2,
            damagePerHit: 3
          },
          {
            name: 'Fortress',
            piece: 'Rook',
            description: 'Gain temporary HP boost',
            mpCost: 8,
            cooldown: 5,
            range: 0,
            isAoe: false,
            tempHpMultiplier: 2,
            durationTurns: 1
          },
          {
            name: 'MagicExplosion',
            piece: 'Queen',
            description: 'Area damage spell',
            mpCost: 10,
            cooldown: 3,
            range: 3,
            isAoe: true,
            damage: 7
          },
          {
            name: 'Resurrection',
            piece: 'Queen',
            description: 'Revive fallen ally',
            mpCost: 12,
            cooldown: 10,
            range: 3,
            isAoe: false,
            heal: 20
          },
          {
            name: 'RoyalCommand',
            piece: 'King',
            description: 'Command nearby units',
            mpCost: 10,
            cooldown: 6,
            range: 8,
            isAoe: false
          },
          {
            name: 'ShieldBash',
            piece: 'Pawn',
            description: 'Basic shield attack',
            mpCost: 2,
            cooldown: 2,
            range: 1,
            isAoe: false,
            damage: 2
          },
          {
            name: 'Breakthrough',
            piece: 'Pawn',
            description: 'Charge attack',
            mpCost: 2,
            cooldown: 2,
            range: 1,
            isAoe: false,
            damage: 3
          }
        ]
      },
      evolution: {
        xpThresholds: { Pawn: 20, Knight: 40, Bishop: 40, Rook: 60, Queen: 0, King: 0 },
        rules: {
          Pawn: ["Knight", "Bishop"],
          Knight: ["Rook"],
          Bishop: ["Rook"],
          Rook: ["Queen"],
          Queen: [],
          King: []
        },
        immediateOnLastRank: { Pawn: true, Knight: false, Bishop: false, Rook: false, Queen: false, King: false }
      },
      ai: {
        nearEvolutionXp: 19,
        lastRankEdgeY: { Elves: 6, Orcs: 1 },
        kingAura: { radius: 3, atkBonus: 1 }
      },
      shieldSystem: {
        king: {
          baseRegen: 5,
          proximityBonus1: { pawn: 1, knight: 2, bishop: 2, rook: 3, queen: 4, king: 0 },
          proximityBonus2: { pawn: 0, knight: 1, bishop: 1, rook: 2, queen: 3, king: 0 }
        },
        ally: {
          neighborContribution: { pawn: 1, knight: 2, bishop: 2, rook: 3, queen: 4, king: 5 }
        }
      },
      killRewards: {
        pawn: 10, knight: 20, bishop: 20, rook: 30, queen: 50, king: 100
      }
    };

    this.configForm.patchValue(defaultConfig);
    
    // Initialize abilities FormArray
    const abilitiesArray = this.configForm.get('abilities.abilities') as FormArray;
    if (abilitiesArray) {
      abilitiesArray.clear();
      defaultConfig.abilities.abilities.forEach((ability: any) => {
        const abilityGroup = this.fb.group({
          name: [ability.name, [Validators.required]],
          piece: [ability.piece, [Validators.required]],
          description: [ability.description || ''],
          mpCost: [ability.mpCost || 0, [Validators.required, Validators.min(0)]],
          cooldown: [ability.cooldown || 0, [Validators.required, Validators.min(0)]],
          range: [ability.range || 0, [Validators.required, Validators.min(0)]],
          isAoe: [ability.isAoe || false],
          damage: [ability.damage || 0, [Validators.min(0)]],
          heal: [ability.heal || 0, [Validators.min(0)]],
          hits: [ability.hits || 0, [Validators.min(0)]],
          damagePerHit: [ability.damagePerHit || 0, [Validators.min(0)]],
          tempHpMultiplier: [ability.tempHpMultiplier || 0, [Validators.min(0)]],
          durationTurns: [ability.durationTurns || 0, [Validators.min(0)]],
          grantsExtraTurn: [ability.grantsExtraTurn || false],
          diagonalOnly: [ability.diagonalOnly || false]
        });
        abilitiesArray.push(abilityGroup);
      });
    }
  }

  // Mark all form controls as touched to show validation errors
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      } else {
        control?.markAsTouched();
      }
    });
  }

  // Get form section by key
  getFormSection(key: string): FormGroup | null {
    return this.configForm.get(key) as FormGroup;
  }

  // Check if form is valid
  get isFormValid(): boolean {
    return this.configForm.valid;
  }

  // Get form errors
  getFormErrors(): string[] {
    const errors: string[] = [];
    this.collectFormErrors(this.configForm, errors);
    return errors;
  }

  private collectFormErrors(formGroup: FormGroup, errors: string[], prefix = ''): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      if (control instanceof FormGroup) {
        this.collectFormErrors(control, errors, prefix ? `${prefix}.${key}` : key);
      } else if (control?.errors) {
        Object.keys(control.errors).forEach(errorKey => {
          errors.push(`${prefix ? `${prefix}.${key}` : key}: ${errorKey}`);
        });
      }
    });
  }
}
