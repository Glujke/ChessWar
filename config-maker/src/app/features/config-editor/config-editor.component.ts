import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatChipsModule } from '@angular/material/chips';
import { MatTabsModule } from '@angular/material/tabs';
import { MatStepperModule } from '@angular/material/stepper';
import { ConfigApiService } from '../../shared/services/config-api.service';
import { BalanceVersion } from '../../shared/models/balance-config.model';
import { ConfigEditorViewModel } from '../../shared/viewmodels/config-editor.viewmodel';

// Form components
import { GlobalsFormComponent } from './forms/globals-form/globals-form.component';
import { PlayerManaFormComponent } from './forms/player-mana-form/player-mana-form.component';
import { PiecesFormComponent } from './forms/pieces-form/pieces-form.component';
import { AbilitiesFormComponent } from './forms/abilities-form/abilities-form.component';
import { EvolutionFormComponent } from './forms/evolution-form/evolution-form.component';
import { AiFormComponent } from './forms/ai-form/ai-form.component';
import { ShieldFormComponent } from './forms/shield-form/shield-form.component';
import { KillRewardsFormComponent } from './forms/kill-rewards-form/kill-rewards-form.component';

@Component({
  selector: 'app-config-editor',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatToolbarModule,
    MatChipsModule,
    MatTabsModule,
    MatStepperModule,
    // Form components
    GlobalsFormComponent,
    PlayerManaFormComponent,
    PiecesFormComponent,
    AbilitiesFormComponent,
    EvolutionFormComponent,
    AiFormComponent,
    ShieldFormComponent,
    KillRewardsFormComponent
  ],
  providers: [ConfigEditorViewModel],
  templateUrl: './config-editor.component.html',
  styleUrl: './config-editor.component.scss'
})
export class ConfigEditorComponent {
  private readonly viewModel = inject(ConfigEditorViewModel);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  // Expose ViewModel properties
  readonly versionId = signal<string>('');
  readonly versionInfo = this.viewModel.versionInfo;
  readonly loading = this.viewModel.loading;
  readonly saving = this.viewModel.saving;
  readonly isEditMode = this.viewModel.isEditMode;
  readonly configForm = this.viewModel.configForm;
  readonly formSections = this.viewModel.formSections;
  readonly errors = this.viewModel.errors;

  // Current active tab
  readonly activeTab = signal(0);

  async ngOnInit() {
    const versionId = this.route.snapshot.paramMap.get('id');
    const isEdit = this.route.snapshot.url.some(segment => segment.path === 'edit');

    // Get version info from navigation state (passed from list)
    const navigation = this.router.getCurrentNavigation();
    const versionFromState = navigation?.extras?.state?.['version'] as BalanceVersion | undefined;
    
    if (versionId) {
      this.versionId.set(versionId);
      this.viewModel.isEditMode.set(isEdit);
      await this.viewModel.loadConfig(versionId, versionFromState);
    }
  }

  async saveConfig() {
    if (this.configForm.invalid) {
      this.snackBar.open('Исправьте ошибки в форме перед сохранением', 'Закрыть', { duration: 5000 });
      return;
    }

    await this.viewModel.saveConfig(this.versionId());
    if (this.viewModel.errors().length === 0) {
      this.snackBar.open('Конфигурация успешно сохранена!', 'Закрыть', { duration: 3000 });
    } else {
      this.snackBar.open('Не удалось сохранить конфигурацию', 'Закрыть', { duration: 5000 });
    }
  }

  async publishConfig() {
    if (!confirm('Вы уверены, что хотите опубликовать эту конфигурацию? Она станет активной версией.')) {
      return;
    }

    await this.viewModel.publishConfig(this.versionId());
    if (this.viewModel.errors().length === 0) {
      this.snackBar.open('Конфигурация успешно опубликована!', 'Закрыть', { duration: 3000 });
      setTimeout(() => {
        this.router.navigate(['/']);
      }, 1000);
    } else {
      this.snackBar.open('Не удалось опубликовать конфигурацию', 'Закрыть', { duration: 5000 });
    }
  }

  onTabChange(index: number) {
    this.activeTab.set(index);
  }

  goBack() {
    this.router.navigate(['/']);
  }

  getStatusColor(status: string): 'primary' | 'accent' | 'warn' {
    switch (status?.toLowerCase()) {
      case 'active': return 'warn';
      case 'published': return 'primary';
      case 'draft': return 'accent';
      default: return 'primary';
    }
  }

  getFormGroup(key: string): FormGroup | null {
    const control = this.configForm.get(key);
    return control instanceof FormGroup ? control : null;
  }

  // Form group getters for type safety
  get globalsForm(): FormGroup {
    return this.configForm.get('globals') as FormGroup;
  }

  get playerManaForm(): FormGroup {
    return this.configForm.get('playerMana') as FormGroup;
  }

  get piecesForm(): FormGroup {
    return this.configForm.get('pieces') as FormGroup;
  }

  get abilitiesForm(): FormGroup {
    return this.configForm.get('abilities') as FormGroup;
  }

  get evolutionForm(): FormGroup {
    return this.configForm.get('evolution') as FormGroup;
  }

  get aiForm(): FormGroup {
    return this.configForm.get('ai') as FormGroup;
  }

  get shieldForm(): FormGroup {
    return this.configForm.get('shieldSystem') as FormGroup;
  }

  get killRewardsForm(): FormGroup {
    return this.configForm.get('killRewards') as FormGroup;
  }
}
