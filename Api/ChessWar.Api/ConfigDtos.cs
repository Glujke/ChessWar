namespace ChessWar.Api;

public record PieceDefinitionDto(string Id, int Hp, int Atk, int Range, int Movement, int Energy, int ExpToEvolve);
public record EvolutionRuleDto(string From, List<string> To);
public record GlobalRulesDto(int MpRegenPerTurn, string CooldownTickPhase);

public record BalanceVersionDto(
  string Version,
  string Status,
  string? Comment,
  List<PieceDefinitionDto> Pieces,
  List<EvolutionRuleDto> EvolutionRules,
  GlobalRulesDto Globals
);

public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);


