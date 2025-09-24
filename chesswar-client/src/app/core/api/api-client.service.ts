import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { GameBoardDto, GameSessionDto, PositionDto, TutorialSessionDto } from './dtos';
import { firstValueFrom } from 'rxjs';
// baseUrl добавляется через перехватчик

export interface StartTutorialResult {
  tutorialId: string;
  gameId: string;
}

export abstract class IApiClientService {
  abstract startTutorial(): Promise<StartTutorialResult>;
  abstract getGameSession(gameId: string): Promise<GameSessionDto>;
  abstract endTurn(gameId: string): Promise<void>;
  abstract getBoard(): Promise<GameBoardDto>;
  abstract getAvailableActions(gameId: string, pieceId: string, actionType: string): Promise<PositionDto[]>;
  abstract movePiece(gameId: string, pieceId: string, target: PositionDto): Promise<void>;
  abstract executeAction(gameId: string, type: 'Attack' | 'Ability', pieceId: string, target: PositionDto, description?: string): Promise<void>;
  abstract makeAiTurn(gameId: string): Promise<void>;
}

@Injectable({ providedIn: 'root' })
export class ApiClientService implements IApiClientService {
  constructor(
    private readonly http: HttpClient = inject(HttpClient)
  ) {}

  async startTutorial(): Promise<StartTutorialResult> {
    const url = `/api/v1/game/tutorial?embed=(game)`;
    const dto = await firstValueFrom(
      this.http.post<any>(url, { playerId: 'web-client', showHints: true })
    );
    const tutorialId: string = dto?.id ?? dto?.tutorialId ?? dto?.tutorialSessionId ?? '';
    const embeddedGameId: string = dto?._embedded?.game?.id ?? '';
    const flatGameId: string = dto?.gameId ?? dto?.gameSessionId ?? '';
    const gameId = embeddedGameId || flatGameId;
    return { tutorialId, gameId };
  }

  async getGameSession(gameId: string): Promise<GameSessionDto> {
    const url = `/api/v1/gamesession/${encodeURIComponent(gameId)}`;
    return await firstValueFrom(this.http.get<GameSessionDto>(url));
  }

  async endTurn(gameId: string): Promise<void> {
    const url = `/api/v1/gamesession/${encodeURIComponent(gameId)}/turn/end`;
    await firstValueFrom(this.http.post<void>(url, {}));
  }

  async getBoard(): Promise<GameBoardDto> {
    const url = `/api/v1/board`;
    return await firstValueFrom(this.http.get<GameBoardDto>(url));
  }

  async getAvailableActions(gameId: string, pieceId: string, actionType: string): Promise<PositionDto[]> {
    const url = `/api/v1/gamesession/${encodeURIComponent(gameId)}/piece/${encodeURIComponent(pieceId)}/actions?actionType=${encodeURIComponent(actionType)}`;
    return await firstValueFrom(this.http.get<PositionDto[]>(url));
  }

  async movePiece(gameId: string, pieceId: string, target: PositionDto): Promise<void> {
    const url = `/api/v1/gamesession/${encodeURIComponent(gameId)}/move`;
    await firstValueFrom(this.http.post<void>(url, { pieceId, targetPosition: target }));
  }

  async executeAction(gameId: string, type: 'Attack' | 'Ability', pieceId: string, target: PositionDto, description?: string): Promise<void> {
    const url = `/api/v1/gamesession/${encodeURIComponent(gameId)}/turn/action`;
    await firstValueFrom(this.http.post<void>(url, { type, pieceId, targetPosition: target, description: description ?? null } as any));
  }

  async makeAiTurn(gameId: string): Promise<void> {
    const url = `/api/v1/gamesession/${encodeURIComponent(gameId)}/turn/ai`;
    await firstValueFrom(this.http.post<void>(url, {}));
  }
}


