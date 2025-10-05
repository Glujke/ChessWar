using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Domain.Services.GameLogic;

/// <summary>
/// Сервис для управления системой "Коллективный Щит"
/// ВСЕ фигуры имеют энергетический щит, зависящий от близости к союзникам
/// </summary>
public class CollectiveShieldService : ICollectiveShieldService
{
    private readonly IBalanceConfigProvider _configProvider;
    private readonly IAttackRulesService _attackRulesService;

    public CollectiveShieldService(
        IBalanceConfigProvider configProvider,
        IAttackRulesService attackRulesService)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _attackRulesService = attackRulesService ?? throw new ArgumentNullException(nameof(attackRulesService));
    }

    /// <summary>
    /// Регенерирует щит короля на основе близости союзников
    /// Король получает щит от ВСЕЙ армии в радиусе ≤2
    /// Конфигурация загружается из BalanceConfig.ShieldSystem.King
    /// </summary>
    /// <param name="king">Король (должен быть типа King)</param>
    /// <param name="allyPieces">Список союзных фигур (включая самого короля опционально)</param>
    /// <returns>Количество восстановленных HP щита</returns>
    public int RegenerateKingShield(Piece king, List<Piece> allyPieces)
    {
        if (king.Type != PieceType.King)
        {
            throw new ArgumentException("Piece must be a King", nameof(king));
        }

        var config = _configProvider.GetActive().ShieldSystem.King;
        var initialShield = king.ShieldHP;
        var totalRegen = config.BaseRegen;

        var allies = allyPieces
            .Where(p => p.Team == king.Team && p.Id != king.Id)
            .ToList();

        foreach (var ally in allies)
        {
            var distance = _attackRulesService.CalculateChebyshevDistance(king.Position, ally.Position);
            
            if (distance <= 2)
            {
                totalRegen += GetProximityBonus(ally.Type, distance);
            }
        }

        var newShield = Math.Min(king.ShieldHP + totalRegen, king.MaxShieldHP);
        var actualRegen = newShield - initialShield;
        
        king.ShieldHP = newShield;
        
       
        var nearbyAllies = allies
            .Where(p => _attackRulesService.CalculateChebyshevDistance(king.Position, p.Position) <= 2)
            .ToList();
        king.NeighborCount = nearbyAllies.Count;

        return actualRegen;
    }

    /// <summary>
    /// Пересчитывает щит обычной фигуры на основе текущих соседей
    /// Shield = СУММА вкладов от всех соседей в радиусе ≤1, но не больше MaxShieldHP
    /// Сосед ушёл → Shield уменьшается, сосед вернулся → Shield увеличивается
    /// Конфигурация загружается из BalanceConfig.ShieldSystem.Ally
    /// </summary>
    /// <param name="ally">Обычная фигура (не King)</param>
    /// <param name="neighbors">Список всех фигур на доске для поиска соседей</param>
    /// <returns>Изменение щита (может быть отрицательным)</returns>
    public int RecalculateAllyShield(Piece ally, List<Piece> neighbors)
    {
        if (ally.Type == PieceType.King)
        {
            throw new ArgumentException("Use RegenerateKingShield for King pieces", nameof(ally));
        }

        var initialShield = ally.ShieldHP;

        var nearbyAllies = neighbors
            .Where(n => n.Team == ally.Team && n.Id != ally.Id)
            .Where(n => _attackRulesService.CalculateChebyshevDistance(ally.Position, n.Position) <= 1)
            .ToList();

        var totalContribution = 0;
        foreach (var neighbor in nearbyAllies)
        {
            totalContribution += GetNeighborContribution(neighbor.Type);
        }

        ally.ShieldHP = Math.Max(0, Math.Min(totalContribution, ally.MaxShieldHP));
        ally.NeighborCount = nearbyAllies.Count;

        return ally.ShieldHP - initialShield;
    }

    /// <summary>
    /// Возвращает бонус к регенерации щита короля от союзника на определённом расстоянии
    /// Значения загружаются из конфигурации
    /// </summary>
    private int GetProximityBonus(PieceType pieceType, int distance)
    {
        var config = _configProvider.GetActive().ShieldSystem.King;
        var pieceTypeName = pieceType.ToString();

        if (distance <= 1)
        {
            return config.ProximityBonus1.GetValueOrDefault(pieceTypeName, 0);
        }

        if (distance == 2)
        {
            return config.ProximityBonus2.GetValueOrDefault(pieceTypeName, 0);
        }

        return 0;
    }

    /// <summary>
    /// Возвращает вклад соседа в щит обычной фигуры (радиус ≤1)
    /// Значения загружаются из конфигурации
    /// </summary>
    private int GetNeighborContribution(PieceType pieceType)
    {
        var config = _configProvider.GetActive().ShieldSystem.Ally;
        var pieceTypeName = pieceType.ToString();
        
        return config.NeighborContribution.GetValueOrDefault(pieceTypeName, 0);
    }
}

