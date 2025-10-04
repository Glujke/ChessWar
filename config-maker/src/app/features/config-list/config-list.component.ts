import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ConfigApiService } from '../../shared/services/config-api.service';
import { BalanceVersion } from '../../shared/models/balance-config.model';
import { CreateVersionDialogComponent } from './create-version-dialog.component';

@Component({
  selector: 'app-config-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatToolbarModule,
    MatSelectModule,
    MatFormFieldModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './config-list.component.html',
  styleUrl: './config-list.component.scss'
})
export class ConfigListComponent {
  private readonly configApi = inject(ConfigApiService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly versions = signal<BalanceVersion[]>([]);
  readonly activeVersionId = signal<string | null>(null);
  readonly loading = signal(true);
  readonly statusFilter = signal<string>('all');
  
  readonly displayedColumns = ['version', 'status', 'createdAt', 'comment', 'actions'];

  readonly filteredVersions = computed(() => {
    const filter = this.statusFilter();
    const all = this.versions();
    if (filter === 'all') return all;
    return all.filter(v => v.status.toLowerCase() === filter.toLowerCase());
  });

  ngOnInit() {
    this.loadVersions();
  }

  loadVersions() {
    this.loading.set(true);
    this.configApi.getVersions().subscribe({
      next: (data) => {
        this.versions.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        this.snackBar.open('Не удалось загрузить версии конфигураций', 'Закрыть', { duration: 5000 });
        console.error('Error loading versions:', error);
        this.loading.set(false);
      }
    });

    this.configApi.getActiveVersion().subscribe({
      next: (active) => {
        this.activeVersionId.set(active.id);
      },
      error: () => {
        this.activeVersionId.set(null);
      }
    });
  }

  viewConfig(versionId: string) {
    const version = this.versions().find(v => v.id === versionId);
    this.router.navigate(['/config', versionId], {
      state: { version }
    });
  }

  editConfig(versionId: string) {
    const version = this.versions().find(v => v.id === versionId);
    this.router.navigate(['/config', versionId, 'edit'], {
      state: { version }
    });
  }

  isActive(versionId: string): boolean {
    return this.activeVersionId() === versionId;
  }

  getStatusColor(status: string): 'primary' | 'accent' | 'warn' {
    switch (status.toLowerCase()) {
      case 'active': return 'warn';
      case 'published': return 'primary';
      case 'draft': return 'accent';
      default: return 'primary';
    }
  }

  onStatusFilterChange(value: string) {
    this.statusFilter.set(value);
  }

  getDefaultConfigJson(): string {
    const defaultConfig = {
      "version": "1.0.0",
      "globals": {
        "mpRegenPerTurn": 10,
        "cooldownTickPhase": "EndTurn"
      },
      "playerMana": {
        "initialMana": 10,
        "maxMana": 50,
        "manaRegenPerTurn": 10,
        "mandatoryAction": true,
        "attackCost": 1,
        "movementCosts": {
          "Pawn": 1,
          "Knight": 2,
          "Bishop": 3,
          "Rook": 3,
          "Queen": 4,
          "King": 4
        }
      },
      "pieces": {
        "Pawn": {
          "hp": 10,
          "atk": 2,
          "range": 1,
          "movement": 1,
          "xpToEvolve": 20,
          "maxShieldHP": 50
        },
        "Knight": {
          "hp": 20,
          "atk": 4,
          "range": 1,
          "movement": 1,
          "xpToEvolve": 40,
          "maxShieldHP": 80
        },
        "Bishop": {
          "hp": 18,
          "atk": 3,
          "range": 4,
          "movement": 8,
          "xpToEvolve": 40,
          "maxShieldHP": 80
        },
        "Rook": {
          "hp": 25,
          "atk": 5,
          "range": 8,
          "movement": 8,
          "xpToEvolve": 60,
          "maxShieldHP": 100
        },
        "Queen": {
          "hp": 30,
          "atk": 7,
          "range": 3,
          "movement": 8,
          "xpToEvolve": 0,
          "maxShieldHP": 150
        },
        "King": {
          "hp": 50,
          "atk": 3,
          "range": 1,
          "movement": 1,
          "xpToEvolve": 0,
          "maxShieldHP": 400
        }
      },
      "abilities": {
        "Bishop.LightArrow": {
          "mpCost": 3,
          "cooldown": 2,
          "range": 4,
          "isAoe": false,
          "damage": 4
        },
        "Bishop.Heal": {
          "mpCost": 6,
          "cooldown": 4,
          "range": 2,
          "isAoe": false,
          "heal": 5
        },
        "Knight.DoubleStrike": {
          "mpCost": 5,
          "cooldown": 3,
          "range": 1,
          "isAoe": false,
          "hits": 2,
          "damagePerHit": 3
        },
        "Rook.Fortress": {
          "mpCost": 8,
          "cooldown": 5,
          "range": 0,
          "isAoe": false,
          "tempHpMultiplier": 2,
          "durationTurns": 1
        },
        "Queen.MagicExplosion": {
          "mpCost": 10,
          "cooldown": 3,
          "range": 3,
          "isAoe": true,
          "damage": 7
        },
        "Queen.Resurrection": {
          "mpCost": 12,
          "cooldown": 10,
          "range": 3,
          "isAoe": false,
          "heal": 20
        },
        "King.RoyalCommand": {
          "mpCost": 10,
          "cooldown": 6,
          "range": 8,
          "isAoe": false
        },
        "Pawn.ShieldBash": {
          "mpCost": 2,
          "cooldown": 2,
          "range": 1,
          "isAoe": false,
          "damage": 2
        },
        "Pawn.Breakthrough": {
          "mpCost": 2,
          "cooldown": 2,
          "range": 1,
          "isAoe": false,
          "damage": 3
        }
      },
      "evolution": {
        "xpThresholds": {
          "Pawn": 20,
          "Knight": 40,
          "Bishop": 40,
          "Rook": 60,
          "Queen": 0,
          "King": 0
        },
        "rules": {
          "Pawn": ["Knight", "Bishop"],
          "Knight": ["Rook"],
          "Bishop": ["Rook"],
          "Rook": ["Queen"],
          "Queen": [],
          "King": []
        },
        "immediateOnLastRank": {
          "Pawn": true
        }
      },
      "ai": {
        "nearEvolutionXp": 19,
        "lastRankEdgeY": {
          "Elves": 6,
          "Orcs": 1
        },
        "kingAura": {
          "radius": 3,
          "atkBonus": 1
        }
      },
      "shieldSystem": {
        "king": {
          "baseRegen": 5,
          "proximityBonus1": {
            "pawn": 1,
            "knight": 2,
            "bishop": 2,
            "rook": 3,
            "queen": 4,
            "king": 0
          },
          "proximityBonus2": {
            "pawn": 0,
            "knight": 1,
            "bishop": 1,
            "rook": 2,
            "queen": 3,
            "king": 0
          }
        },
        "ally": {
          "neighborContribution": {
            "pawn": 1,
            "knight": 2,
            "bishop": 2,
            "rook": 3,
            "queen": 4,
            "king": 5
          }
        }
      },
      "killRewards": {
        "pawn": 10,
        "knight": 20,
        "bishop": 20,
        "rook": 30,
        "queen": 50,
        "king": 100
      }
    };
    
    return JSON.stringify(defaultConfig, null, 2);
  }

  createVersion() {
    const dialogRef = this.dialog.open(CreateVersionDialogComponent, {
      width: '500px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.configApi.createVersion(result.version, result.comment).subscribe({
          next: (version) => {
            // Create default payload immediately after version creation
            this.configApi.savePayload(version.id, this.getDefaultConfigJson()).subscribe({
              next: () => {
                this.snackBar.open(`Версия ${result.version} создана с конфигурацией по умолчанию!`, 'Закрыть', { duration: 3000 });
                this.loadVersions();
              },
              error: (error) => {
                this.snackBar.open('Версия создана, но не удалось добавить конфигурацию по умолчанию', 'Закрыть', { duration: 5000 });
                console.error('Error creating default payload:', error);
                this.loadVersions();
              }
            });
          },
          error: (error) => {
            this.snackBar.open('Не удалось создать версию', 'Закрыть', { duration: 5000 });
            console.error('Error creating version:', error);
          }
        });
      }
    });
  }
}
