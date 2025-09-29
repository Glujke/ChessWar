import { TestBed } from '@angular/core/testing';
import { GameViewModel } from './game.view-model';

describe('GameViewModel (unit)', () => {
  let vm: GameViewModel;

  const apiMock = {
    getGameSession: jest.fn(),
    getBoard: jest.fn(),
    endTurn: jest.fn(),
    getAvailableActions: jest.fn(),
    movePiece: jest.fn(),
    executeAction: jest.fn(),
    getAbilityTargets: jest.fn(),
    evolve: jest.fn(),
    tutorialTransition: jest.fn()
  } as any;

  const hubMock = {
    connect: jest.fn(async () => {}),
    joinGame: jest.fn(async () => {}),
    on: jest.fn(),
    off: jest.fn()
  } as any;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        GameViewModel,
        { provide: (apiMock as any).constructor, useValue: apiMock },
        { provide: (hubMock as any).constructor, useValue: hubMock },
        { provide: 'ApiClientService', useValue: apiMock },
        { provide: 'GameHubService', useValue: hubMock },
      ]
    });
    // Provide tokens by class name used inside VM via inject
    (TestBed as any).overrideProvider(require('../../core/api/api-client.service').ApiClientService, { useValue: apiMock });
    (TestBed as any).overrideProvider(require('../../core/signalr/game-hub.service').GameHubService, { useValue: hubMock });
    vm = TestBed.inject(GameViewModel);
    jest.clearAllMocks();
  });

  it('enableHints(reset) shows and resets to step 1', () => {
    vm.enableHints(true);
    expect(vm.showHints()).toBe(true);
    expect(vm.tutorialStep()).toBe(1);
  });

  it('selectPiece advances tutorial from 1 to 2', async () => {
    // Arrange: minimal session/board state
    vm.session.set({ player1: { id: 'p1', pieces: [{ id: 'X', type: 'Pawn', position: { x: 0, y: 1 } }] }, player2: { id: 'p2', pieces: [] } } as any);
    vm.board.set({ pieces: [{ id: 'X', type: 'Pawn', position: { x: 0, y: 1 } }] } as any);
    (apiMock.getAvailableActions as jest.Mock).mockResolvedValueOnce([{ x: 0, y: 2 }]).mockResolvedValueOnce([]);

    // Act
    await vm.selectPiece('g', 'X');

    // Assert
    expect(vm.tutorialStep()).toBe(2);
    expect(vm.highlighted().length).toBeGreaterThanOrEqual(1);
  });

  it('moveSelectedTo advances tutorial from 2 to 3 and clears highlights', async () => {
    vm.tutorialStep.set(2);
    vm.selectedPieceId.set('X');
    (apiMock.movePiece as jest.Mock).mockResolvedValueOnce({});
    (apiMock.getGameSession as jest.Mock).mockResolvedValueOnce({ player1: { id: 'p1', pieces: [{ id: 'X', type: 'Pawn', position: { x: 0, y: 2 } }] }, player2: { id: 'p2', pieces: [] } });

    await vm.moveSelectedTo('g', { x: 0, y: 2 } as any);

    expect(vm.tutorialStep()).toBe(3);
    expect(vm.highlighted().length).toBe(0);
  });

  it('endTurn resets tutorial step from 3 to 1 when turn returns to player', async () => {
    vm.tutorialStep.set(3);
    (apiMock.endTurn as jest.Mock).mockResolvedValueOnce({});
    (apiMock.getGameSession as jest.Mock)
      .mockResolvedValueOnce({ currentTurn: { activeParticipant: { id: 'ai' } }, player1: { id: 'p1', pieces: [] }, player2: { id: 'ai', pieces: [] } })
      .mockResolvedValueOnce({ currentTurn: { activeParticipant: { id: 'p1' } }, player1: { id: 'p1', pieces: [] }, player2: { id: 'ai', pieces: [] } });

    await vm.endTurn('g');

    expect(vm.tutorialStep()).toBe(1);
    expect(vm.error()).toBeNull();
  });

  it('filters dead pieces (isAlive=false, hp<=0, or without position) on moveSelectedTo()', async () => {
    vm.selectedPieceId.set('A1');
    (apiMock.movePiece as jest.Mock).mockResolvedValueOnce({});
    (apiMock.getGameSession as jest.Mock).mockResolvedValueOnce({
      player1: {
        id: 'p1',
        pieces: [
          { id: 'A1', type: 'Pawn', hp: 10, position: { x: 1, y: 1 } },
          { id: 'D1', type: 'Pawn', hp: 0, isAlive: false, position: { x: 2, y: 2 } }, // corpse: hp=0 & isAlive=false
        ]
      },
      player2: {
        id: 'p2',
        pieces: [
          { id: 'E1', type: 'Pawn', hp: 0, isAlive: false, position: { x: 3, y: 3 } }, // corpse
          { id: 'F1', type: 'Pawn', hp: 5 } // filtered by missing position
        ]
      }
    });

    await vm.moveSelectedTo('g', { x: 1, y: 1 } as any);

    const b = vm.board();
    expect(b).toBeTruthy();
    const ids = (b as any).pieces.map((p: any) => String(p.id));
    expect(ids).toEqual(['A1']);
  });

  it('filters dead pieces on attackTarget()', async () => {
    vm.selectedPieceId.set('A1');
    (apiMock.executeAction as jest.Mock).mockResolvedValueOnce({});
    (apiMock.getGameSession as jest.Mock).mockResolvedValueOnce({
      player1: { id: 'p1', pieces: [{ id: 'A1', type: 'Pawn', hp: 10, position: { x: 1, y: 1 } }] },
      player2: { id: 'p2', pieces: [{ id: 'B2', type: 'Pawn', hp: 0, isAlive: false, position: { x: 2, y: 2 } }] }
    });

    await vm.attackTarget('g', { x: 2, y: 2 } as any);
    const ids = (vm.board() as any).pieces.map((p: any) => String(p.id));
    expect(ids).toEqual(['A1']);
  });

  it('updates board on PieceEvolved signal', async () => {
    // Arrange: минимальная сессия для load()
    (apiMock.getGameSession as jest.Mock).mockResolvedValue({ player1: { id: 'p1', pieces: [{ id: 'A1', type: 'Pawn', position: { x: 1, y: 1 } }] }, player2: { id: 'p2', pieces: [] } });
    // Act: вызов load регистрирует обработчик PieceEvolved
    await vm.load('g');
    const payload = { pieceId: 'A1', newType: 'Bishop', position: { x: 1, y: 1 } };
    const handler = (hubMock.on as jest.Mock).mock.calls.find((c: any[]) => c[0] === 'PieceEvolved')?.[1];
    expect(typeof handler).toBe('function');
    handler(payload);

    const s = vm.session() as any;
    expect(s.player1.pieces[0].type).toBe('Bishop');
    expect(s.player1.pieces[0].position).toEqual({ x: 1, y: 1 });
  });

  it('confirmEvolution does not hide evolved piece on first update even if corpse flags are set', async () => {
    // Arrange: selected piece A1 is ready to evolve
    vm.selectedPieceId.set('A1');
    // evolve returns piece with correct position, но с неконсистентными флагами hp=0,isAlive=false
    (apiMock.evolve as jest.Mock).mockResolvedValueOnce({
      player1: { id: 'p1', pieces: [{ id: 'A1', type: 'Bishop', hp: 0, isAlive: false, position: { x: 4, y: 4 } }] },
      player2: { id: 'p2', pieces: [] }
    });

    // Act
    await vm.confirmEvolution('g', 'Bishop');

    // Assert: на первом апдейте фигура всё равно на доске (одноразовое исключение)
    const b = vm.board();
    expect(b).toBeTruthy();
    const ids = (b as any).pieces.map((p: any) => String(p.id));
    expect(ids).toEqual(['A1']);
  });

  it('corpses are not rendered even if they still have position', async () => {
    // Arrange: загрузка сессии
    (apiMock.getGameSession as jest.Mock).mockResolvedValue({
      player1: { id: 'p1', pieces: [
        { id: 'L1', type: 'Pawn', hp: 0, isAlive: false, position: { x: 2, y: 2 } },
        { id: 'A1', type: 'Pawn', hp: 10, position: { x: 1, y: 1 } },
      ] },
      player2: { id: 'p2', pieces: [] }
    });

    await vm.load('g');

    const ids = (vm.board() as any).pieces.map((p: any) => String(p.id));
    expect(ids).toEqual(['A1']);
  });

  it('logs attack details with damage and death', async () => {
    // Arrange: начальное состояние
    vm.session.set({
      player1: { id: 'p1', pieces: [{ id: 'A1', type: 'Pawn', hp: 10, position: { x: 1, y: 1 } }] },
      player2: { id: 'p2', pieces: [{ id: 'B2', type: 'Pawn', hp: 5, position: { x: 2, y: 2 } }] }
    } as any);
    vm.selectedPieceId.set('A1');
    
    // После атаки: цель убита
    (apiMock.executeAction as jest.Mock).mockResolvedValueOnce({});
    (apiMock.getGameSession as jest.Mock).mockResolvedValueOnce({
      player1: { id: 'p1', pieces: [{ id: 'A1', type: 'Pawn', hp: 10, position: { x: 2, y: 2 } }] },
      player2: { id: 'p2', pieces: [] } // B2 убит
    });

    // Act
    await vm.attackTarget('g', { x: 2, y: 2 } as any);

    // Assert: проверяем логи
    const logs = vm.logs();
    const combatLog = logs.find(l => l.source === 'COMBAT');
    expect(combatLog).toBeTruthy();
    expect(combatLog?.message).toContain('💀');
    expect(combatLog?.message).toContain('убил');
    expect(combatLog?.message).toContain('урон: 5');
  });

  it('logs ability details with healing', async () => {
    // Arrange: начальное состояние
    vm.session.set({
      player1: { id: 'p1', pieces: [{ id: 'A1', type: 'Bishop', hp: 10, position: { x: 1, y: 1 } }] },
      player2: { id: 'p2', pieces: [{ id: 'B2', type: 'Pawn', hp: 3, position: { x: 2, y: 2 } }] }
    } as any);
    vm.selectedPieceId.set('A1');
    vm.selectedAbility.set('Heal');
    
    // После способности: цель вылечена
    (apiMock.executeAction as jest.Mock).mockResolvedValueOnce({});
    (apiMock.getGameSession as jest.Mock).mockResolvedValueOnce({
      player1: { id: 'p1', pieces: [{ id: 'A1', type: 'Bishop', hp: 10, position: { x: 1, y: 1 } }] },
      player2: { id: 'p2', pieces: [{ id: 'B2', type: 'Pawn', hp: 6, position: { x: 2, y: 2 } }] } // +3 HP
    });

    // Act
    await vm.useAbility('g', { x: 2, y: 2 } as any);

    // Assert: проверяем логи
    const logs = vm.logs();
    const abilityLog = logs.find(l => l.source === 'ABILITY');
    expect(abilityLog).toBeTruthy();
    expect(abilityLog?.message).toContain('💚');
    expect(abilityLog?.message).toContain('лечение +3');
  });
});


