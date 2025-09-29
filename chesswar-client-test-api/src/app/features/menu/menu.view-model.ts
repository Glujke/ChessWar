import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ApiClientService, IApiClientService } from '../../core/api/api-client.service';

@Injectable({ providedIn: 'root' })
export class MenuViewModel {
  public readonly isTutorialEnabled = signal(true);

  constructor(
    private readonly api: IApiClientService = inject(ApiClientService),
    private readonly router: Router = inject(Router)
  ) {}

  async onClickTutorial(): Promise<void> {
    const { gameId } = await this.api.startTutorial();
    if (gameId) {
      await this.router.navigate(['/gamesession', gameId]);
    }
  }
}


