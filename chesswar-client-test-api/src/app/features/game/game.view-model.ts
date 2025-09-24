import { computed, inject, Injectable, signal } from '@angular/core';
import { IApiClientService, ApiClientService } from '../../core/api/api-client.service';
import { GameHubService } from '../../core/signalr/game-hub.service';
import { GameBoardDto, GameSessionDto, PieceDto, PositionDto } from '../../core/api/dtos';

@Injectable({ providedIn: 'root' })
export class GameViewModel {
  private readonly api: IApiClientService = inject(ApiClientService);
  private readonly hub = inject(GameHubService);

  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);
  readonly session = signal<GameSessionDto | null>(null);
  readonly board = signal<GameBoardDto | null>(null);
  readonly highlighted = signal<PositionDto[]>([]); // moves
  readonly highlightedAttacks = signal<PositionDto[]>([]);
  readonly selectedAbility = signal<string | null>(null);
  readonly abilityTargets = signal<PositionDto[]>([]);
  readonly selectedPiece = signal<PieceDto | null>(null);
  readonly selectedPieceId = signal<string | null>(null);
  readonly evolutionChoice = signal<'Knight' | 'Bishop' | null>(null);
  readonly isEvolutionDialogOpen = signal(false);
  readonly isGameFinished = signal(false);
  readonly gameResult = signal<string | null>(null);
  // Tutorial hints
  readonly tutorialStep = signal<1 | 2 | 3 | 4>(1); // 1: select, 2: move/attack, 3: end turn, 4: ability
  readonly showHints = signal(true);
  // Log panel
  readonly logs = signal<{ ts: string; level: 'info' | 'error' | 'event'; source: string; message: string; data?: unknown }[]>([]);

  private addLog(level: 'info' | 'error' | 'event', source: string, message: string, data?: unknown): void {
    const ts = new Date().toLocaleTimeString();
    const entry = { ts, level, source, message, data } as const;
    this.logs.update(list => [entry, ...list].slice(0, 200));
  }
  clearLogs(): void { this.logs.set([]); }

  readonly manaText = computed(() => {
    const s = this.session();
    // Отображаем ману активного игрока (Player1). Из-за разных правил сериализации проверяем несколько вариантов ключей
    const p = (s as any)?.player1 ?? {};
    const current = p.mp ?? p.MP ?? p.mP ?? 0;
    const max = p.maxMp ?? p.MaxMP ?? p.maxMP ?? 0;
    return `${current}/${max}`;
  });

  readonly isMyTurn = computed(() => {
    const s = this.session();
    if (!s) return false;
    const activeId = s.currentTurn?.activeParticipant?.id ?? s.player1?.id;
    return activeId === s.player1?.id;
  });

  readonly isPlayersPiece = (piece: PieceDto | null): boolean => {
    const s = this.session();
    if (!piece || !s) return false;
    const activeId = s.currentTurn?.activeParticipant?.id ?? s.player1?.id;
    const isActivePlayer1 = activeId === s.player1?.id;
    const base = isActivePlayer1 ? (s.player1?.pieces ?? []) : (s.player2?.pieces ?? []);
    const playerPieces = new Set(base.map(p => String(p.id)));
    return playerPieces.has(String(piece.id));
  };

  async load(gameId: string): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      this.addLog('info', 'VM', 'Loading game session', { gameId });
      const data = await this.api.getGameSession(gameId);
      this.session.set(data);
      // Предпочтем доску из сессии (через фигуры игроков), fallback — бэкендовый /board
      const playerPieces = [...(data.player1?.pieces ?? []), ...(data.player2?.pieces ?? [])];
      if (playerPieces.length > 0) {
        this.board.set({ pieces: playerPieces } as any);
      } else {
        const board = await this.api.getBoard();
        this.board.set(board);
      }
      if ((data as any).status === 'Finished' || (data as any).status === 'finished') {
        this.isGameFinished.set(true);
        this.gameResult.set((data as any).result ?? null);
      }

      // Подключение к SignalR и подписка на события
      await this.hub.connect(gameId);
      await this.hub.joinGame(gameId);
      const refresh = async () => {
        const updated = await this.api.getGameSession(gameId);
        this.session.set(updated);
        const pieces = [...(updated.player1?.pieces ?? []), ...(updated.player2?.pieces ?? [])];
        this.board.set({ pieces } as any);
        if ((updated as any).status === 'Finished' || (updated as any).status === 'finished') {
          this.isGameFinished.set(true);
          this.gameResult.set((updated as any).result ?? null);
        }
      };
      this.hub.on('AiMove', refresh);
      this.hub.on('GameEnded', async () => {
        this.addLog('event', 'SignalR', 'GameEnded');
        await refresh();
      });
      // Универсальный слушатель: на любое событие обновляем сессию
      (this.hub as any).onAny?.(async (name: string, payload: unknown) => {
        this.addLog('event', 'SignalR', String(name), payload);
        await refresh();
      });
      this.addLog('info', 'VM', 'Game session loaded');
    } catch (e: any) {
      const msg = e?.problem?.title ?? e?.message ?? 'Не удалось загрузить сессию';
      this.error.set(msg);
      this.addLog('error', 'VM', 'Load failed', e);
    } finally {
      this.isLoading.set(false);
    }
  }

  async endTurn(gameId: string): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      await this.api.endTurn(gameId);
      this.addLog('info', 'HTTP', 'POST /turn/end ok', { gameId });
      // После завершения хода сервер сам выполнит ход ИИ и восстановит ману.
      // Ждём, пока активный участник снова станет player1, с тайм‑аутом.
      const start = Date.now();
      const timeoutMs = 4000;
      let updated = await this.api.getGameSession(gameId);
      while ((updated.currentTurn?.activeParticipant?.id ?? updated.player1?.id) !== updated.player1?.id) {
        if (Date.now() - start > timeoutMs) break;
        await new Promise(r => setTimeout(r, 300));
        updated = await this.api.getGameSession(gameId);
      }
      this.session.set(updated);
      const pieces = [...(updated.player1?.pieces ?? []), ...(updated.player2?.pieces ?? [])];
      this.board.set({ pieces } as any);
      if ((updated as any).status === 'Finished' || (updated as any).status === 'finished') {
        this.isGameFinished.set(true);
        this.gameResult.set((updated as any).result ?? null);
      }
    } catch (e: any) {
      // Если сервер вернул ошибку, всё равно попробуем обновить сессию:
      try {
        const updated = await this.api.getGameSession(gameId);
        this.session.set(updated);
        const pieces = [...(updated.player1?.pieces ?? []), ...(updated.player2?.pieces ?? [])];
        this.board.set({ pieces } as any);
        const finished = (updated as any).status === 'Finished' || (updated as any).status === 'finished';
        if (finished) {
          this.isGameFinished.set(true);
          this.gameResult.set((updated as any).result ?? null);
          // Ошибку не показываем, если игра реально завершена
          this.error.set(null);
          this.addLog('info', 'HTTP', 'POST /turn/end -> finished despite error', { gameId });
          return;
        }
      } catch {
        // ignore
      }
      const msg = e?.problem?.title ?? e?.message ?? 'Не удалось завершить ход';
      this.error.set(msg);
      this.addLog('error', 'HTTP', 'POST /turn/end failed', e);
    } finally {
      this.isLoading.set(false);
    }
  }

  async selectPiece(gameId: string, pieceId: string): Promise<void> {
    this.selectedPiece.set(null as any);
    this.highlighted.set([]);
    this.highlightedAttacks.set([]);
    this.selectedAbility.set(null);
    this.abilityTargets.set([]);
    try {
      this.selectedPieceId.set(pieceId);
      // Diagnostics: log request and responses for Move/Attack
      // eslint-disable-next-line no-console
      console.debug('[VM/selectPiece] pieceId=', pieceId);
      this.addLog('info', 'HTTP', 'GET actions Move/Attack', { pieceId });
      const [moves, attacks] = await Promise.all([
        this.api.getAvailableActions(gameId, pieceId, 'Move'),
        this.api.getAvailableActions(gameId, pieceId, 'Attack')
      ]);
      this.addLog('info', 'HTTP', 'actions received', { moves, attacks });
      // eslint-disable-next-line no-console
      console.debug('[VM/selectPiece] moves=', moves);
      // eslint-disable-next-line no-console
      console.debug('[VM/selectPiece] attacks=', attacks);
      this.highlighted.set(moves ?? []);
      this.highlightedAttacks.set(attacks ?? []);
      const board = this.board();
      const found = board?.pieces.find(p => String(p.id) === String(pieceId)) ?? null;
      // eslint-disable-next-line no-console
      console.debug('[VM/selectPiece] found piece on board =', found);
      this.selectedPiece.set(found);
      // Hints: after selecting a piece, advance from step 1 to 2
      if (this.showHints() && this.tutorialStep() === 1) {
        this.tutorialStep.set(2);
      }
    } catch (e) {
      // ignore highlighting errors for now
      this.addLog('error', 'VM', 'selectPiece failed', e as any);
    }
  }

  async moveSelectedTo(gameId: string, target: PositionDto): Promise<void> {
    const pieceId = this.selectedPieceId();
    if (!pieceId) return;
    try {
      await this.api.movePiece(gameId, pieceId, target);
      this.addLog('info', 'HTTP', 'POST /move ok', { pieceId, target });
      const updated = await this.api.getGameSession(gameId);
      this.session.set(updated);
      const playerPieces = [...(updated.player1?.pieces ?? []), ...(updated.player2?.pieces ?? [])];
      this.board.set({ pieces: playerPieces } as any);
      if ((updated as any).status === 'Finished' || (updated as any).status === 'finished') {
        this.isGameFinished.set(true);
        this.gameResult.set((updated as any).result ?? null);
      }
      // Проверяем необходимость эволюции сразу после перемещения
      const moved = playerPieces.find(p => String(p.id) === String(pieceId)) ?? null;
      this.checkEvolutionNeed(moved as any);
      this.highlighted.set([]);
      // Оставим выбранной фигуру, чтобы подтвердить эволюцию без повторного выбора
      this.selectedPiece.set(moved ?? null);
      this.selectedPieceId.set(String(pieceId));
      if (this.showHints() && this.tutorialStep() === 2) {
        this.tutorialStep.set(3);
      }
    } catch (e: any) {
      this.error.set(e?.problem?.title ?? e?.message ?? 'Не удалось переместить фигуру');
      this.addLog('error', 'HTTP', 'POST /move failed', e);
    }
  }

  async attackTarget(gameId: string, target: PositionDto): Promise<void> {
    const pieceId = this.selectedPieceId();
    if (!pieceId) return;
    try {
      await this.api.executeAction(gameId, 'Attack', pieceId, target);
      this.addLog('info', 'HTTP', 'POST /turn/action Attack ok', { pieceId, target });
      const updated = await this.api.getGameSession(gameId);
      this.session.set(updated);
      const playerPieces = [...(updated.player1?.pieces ?? []), ...(updated.player2?.pieces ?? [])];
      this.board.set({ pieces: playerPieces } as any);
      if ((updated as any).status === 'Finished' || (updated as any).status === 'finished') {
        this.isGameFinished.set(true);
        this.gameResult.set((updated as any).result ?? null);
      }
      this.highlighted.set([]);
      this.highlightedAttacks.set([]);
      this.selectedAbility.set(null);
      this.abilityTargets.set([]);
      this.selectedPiece.set(null);
      this.selectedPieceId.set(null);
      if (this.showHints() && this.tutorialStep() === 2) {
        this.tutorialStep.set(3);
      }
    } catch (e: any) {
      this.error.set(e?.problem?.title ?? e?.message ?? 'Не удалось выполнить атаку');
      this.addLog('error', 'HTTP', 'POST /turn/action Attack failed', e);
    }
  }

  getAbilitiesForSelected(): { name: string; manaCost: number; range?: number; cooldown?: number }[] {
    const piece = this.selectedPiece();
    if (!piece) return [];
    const type = typeof (piece as any).type === 'number' ? this.mapEnumToName((piece as any).type as number) : String((piece as any).type);
    const cooldowns = (piece as any).abilityCooldowns as Record<string, number> | undefined;
    const cd = (name: string) => (cooldowns?.[name] ?? 0);
    switch (type) {
      case 'Pawn':
        return [
          { name: 'ShieldBash', manaCost: 2, range: 1, cooldown: cd('ShieldBash') },
          { name: 'Breakthrough', manaCost: 2, range: 1, cooldown: cd('Breakthrough') }
        ];
      case 'Knight':
        return [
          { name: 'DoubleStrike', manaCost: 5, range: 1, cooldown: cd('DoubleStrike') },
          { name: 'IronStance', manaCost: 4, range: 0, cooldown: cd('IronStance') }
        ];
      case 'Bishop':
        return [
          { name: 'LightArrow', manaCost: 3, range: 4, cooldown: cd('LightArrow') },
          { name: 'Heal', manaCost: 6, range: 2, cooldown: cd('Heal') }
        ];
      case 'Rook':
        return [
          { name: 'ArrowVolley', manaCost: 7, range: 8, cooldown: cd('ArrowVolley') },
          { name: 'Fortress', manaCost: 8, range: 0, cooldown: cd('Fortress') }
        ];
      case 'Queen':
        return [
          { name: 'MagicBurst', manaCost: 10, range: 3, cooldown: cd('MagicBurst') },
          { name: 'Resurrection', manaCost: 12, range: 0, cooldown: cd('Resurrection') }
        ];
      case 'King':
        return [
          { name: 'RoyalDecree', manaCost: 10, range: 0, cooldown: cd('RoyalDecree') }
        ];
      default:
        return [];
    }
  }

  async showAbilityTargets(gameId: string, abilityName: string): Promise<void> {
    const pieceId = this.selectedPieceId();
    if (!pieceId) return;
    this.selectedAbility.set(abilityName);
    try {
      const targets = await this.api.getAbilityTargets(gameId, pieceId, abilityName);
      this.abilityTargets.set(targets ?? []);
      this.highlightedAttacks.set(targets ?? []); // используем красную подсветку для целей способности
      this.addLog('info', 'HTTP', 'GET actions Ability targets', { abilityName, targets });
    } catch {
      this.abilityTargets.set([]);
      this.highlightedAttacks.set([]);
      this.addLog('error', 'HTTP', 'GET actions Ability targets failed', { abilityName });
    }
    if (this.showHints() && this.tutorialStep() < 4) {
      this.tutorialStep.set(4);
    }
  }

  async useAbility(gameId: string, target: PositionDto): Promise<void> {
    const pieceId = this.selectedPieceId();
    const ability = this.selectedAbility();
    if (!pieceId || !ability) return;
    try {
      await this.api.executeAction(gameId, 'Ability', pieceId, target, ability);
      this.addLog('info', 'HTTP', 'POST /turn/action Ability ok', { ability, pieceId, target });
      const updated = await this.api.getGameSession(gameId);
      this.session.set(updated);
      const playerPieces = [...(updated.player1?.pieces ?? []), ...(updated.player2?.pieces ?? [])];
      this.board.set({ pieces: playerPieces } as any);
      if ((updated as any).status === 'Finished' || (updated as any).status === 'finished') {
        this.isGameFinished.set(true);
        this.gameResult.set((updated as any).result ?? null);
      }
    } finally {
      this.selectedAbility.set(null);
      this.abilityTargets.set([]);
      this.highlightedAttacks.set([]);
    }
  }

  async replayTutorial(gameId: string): Promise<string | null> {
    try {
      const res: any = await (this.api as any).tutorialTransition(gameId, 'replay');
      const nextId: string | undefined = res?.gameSessionId ?? res?._embedded?.game?.id;
      return nextId ?? null;
    } catch {
      return null;
    }
  }

  private mapEnumToName(enumValue: number): string {
    const map: Record<number, string> = { 0: 'Pawn', 1: 'Knight', 2: 'Bishop', 3: 'Rook', 4: 'Queen', 5: 'King' };
    return map[enumValue] ?? String(enumValue);
  }

  // Триггер показа диалога при достижении последней линии или XP-порога
  checkEvolutionNeed(piece: PieceDto | null): void {
    if (!piece) return;
    const s = this.session();
    if (!s) return;
    const pos = (piece as any).position as PositionDto | undefined;
    const xp = (piece as any).xp as number | undefined;
    const xpToEvolve = (piece as any).xpToEvolve as number | undefined;
    const isPawn = String((piece as any).type) === 'Pawn' || (piece as any).type === 0;
    if (!isPawn) return;
    const reachedLastRank = pos ? (pos.y === 7) : false; // для эльфов вершина по README — увеличить при инверсии оси при необходимости
    const xpReady = xpToEvolve !== undefined && xp !== undefined && xp >= xpToEvolve;
    if (reachedLastRank || xpReady) {
      this.isEvolutionDialogOpen.set(true);
    }
  }

  async confirmEvolution(gameId: string, choice: 'Knight' | 'Bishop'): Promise<void> {
    const pieceId = this.selectedPieceId();
    if (!pieceId) return;
    this.isLoading.set(true);
    this.error.set(null);
    try {
      const updated = await this.api.evolve(gameId, pieceId, choice);
      this.addLog('info', 'HTTP', 'POST /evolve ok', { pieceId, choice });
      this.session.set(updated);
      const pieces = [...(updated.player1?.pieces ?? []), ...(updated.player2?.pieces ?? [])];
      this.board.set({ pieces } as any);
      this.isEvolutionDialogOpen.set(false);
      this.evolutionChoice.set(null);
    } catch (e: any) {
      this.error.set(e?.problem?.title ?? e?.message ?? 'Не удалось выполнить эволюцию');
      this.addLog('error', 'HTTP', 'POST /evolve failed', e);
    } finally {
      this.isLoading.set(false);
    }
  }

  // Tutorial hint helpers
  nextHint(): void {
    const s = this.tutorialStep();
    if (s < 4) this.tutorialStep.set((s + 1) as any);
  }
  skipHints(): void {
    this.showHints.set(false);
  }
}


