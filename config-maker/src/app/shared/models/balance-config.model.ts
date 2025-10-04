export interface BalanceVersion {
  id: string;
  version: string;
  status: 'Active' | 'Published' | 'Draft';
  comment: string | null;
  createdAt: string;
  publishedAt?: string | null;
}

export interface BalancePayload {
  versionId: string;
  json: string;
}

export interface BalanceConfig {
  globals: GlobalsSection;
  playerMana: PlayerManaSection;
  pieces: Record<string, PieceStats>;
  abilities: Record<string, AbilitySpecModel>;
  evolution: EvolutionSection;
  ai: AiSection;
  shieldSystem: ShieldSystemConfig;
  killRewards: KillRewardsSection;
}

export interface GlobalsSection {
  mpRegenPerTurn: number;
  cooldownTickPhase: string;
}

export interface PlayerManaSection {
  initialMana: number;
  maxMana: number;
  manaRegenPerTurn: number;
  mandatoryAction: boolean;
  attackCost: number;
  movementCosts: Record<string, number>;
}

export interface PieceStats {
  hp: number;
  atk: number;
  range: number;
  movement: number;
  xpToEvolve: number;
  maxShieldHP: number;
}

export interface AbilitySpecModel {
  mpCost: number;
  cooldown: number;
  range: number;
  isAoe: boolean;
  damage?: number;
}

export interface EvolutionSection {
  xpThresholds: Record<string, number>;
  rules: Record<string, string[]>;
  immediateOnLastRank?: Record<string, boolean>;
}

export interface AiSection {
  nearEvolutionXp: number;
  lastRankEdgeY?: Record<string, number>;
  kingAura?: KingAuraConfig;
}

export interface KingAuraConfig {
  radius: number;
  atkBonus: number;
}

export interface ShieldSystemConfig {
  king: KingShieldConfig;
  ally: AllyShieldConfig;
}

export interface KingShieldConfig {
  baseRegen: number;
  proximityBonus1: Record<string, number>;
  proximityBonus2: Record<string, number>;
}

export interface AllyShieldConfig {
  neighborContribution: Record<string, number>;
}

export interface KillRewardsSection {
  pawn: number;
  knight: number;
  bishop: number;
  rook: number;
  queen: number;
  king: number;
}

