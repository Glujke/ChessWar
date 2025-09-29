export interface GameSessionDto {
  id: string;
  player1: PlayerDto;
  player2: PlayerDto;
  status: string;
  result?: string;
  currentTurn?: TurnDto;
  mode?: string;
}

export interface PlayerDto {
  id: string;
  name: string;
  pieces: PieceDto[];
  victories: number;
  createdAt: string;
  mp: number;
  maxMp: number;
}

export interface TutorialSessionDto {
  id: string;
  _embedded?: {
    game?: GameSessionDto;
  };
}

export interface PositionDto {
  x: number;
  y: number;
}

export interface PieceDto {
  id: number | string;
  type: string | number;
  team?: string | number;
  hp?: number;
  atk?: number;
  attackRange?: number;
  movement?: number;
  position?: PositionDto;
  xp?: number;
  xpToEvolve?: number;
  isAlive?: boolean;
  isFirstMove?: boolean;
  abilityCooldowns?: Record<string, number>;
}

export interface GameBoardDto {
  size?: number;
  width?: number;
  height?: number;
  pieces: PieceDto[];
}

export interface TurnDto {
  number: number;
  activeParticipant: PlayerDto;
  selectedPiece?: PieceDto;
  createdAt: string;
}

export type ActionKind = 'Move' | 'Attack' | 'Ability';

export interface AvailableActionDto {
  kind: ActionKind;
  pieceId: string;
  target?: PositionDto;
  abilityId?: string;
  manaCost?: number;
}

export interface AvailableActionsResponseDto {
  pieceId: string;
  available: AvailableActionDto[];
  highlightCells?: PositionDto[];
}


