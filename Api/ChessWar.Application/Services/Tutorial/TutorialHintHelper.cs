using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.Tutorial;

namespace ChessWar.Application.Services.Tutorial;

/// <summary>
/// Вспомогательный класс для генерации подсказок обучения
/// </summary>
public class TutorialHintHelper
{
    public List<string> GetHintsForStage(TutorialStage stage)
    {
        return stage switch
        {
            TutorialStage.Introduction => new List<string>
            {
                "Добро пожаловать в обучение!",
                "Изучите основы игры в шахматы",
                "Используйте подсказки для навигации"
            },
            TutorialStage.Battle1 => new List<string>
            {
                "Ваш первый бой с ИИ!",
                "Попробуйте атаковать вражеские фигуры",
                "Следите за здоровьем своих фигур"
            },
            TutorialStage.Battle2 => new List<string>
            {
                "Средняя сложность ИИ",
                "Используйте способности фигур",
                "Планируйте ходы заранее"
            },
            TutorialStage.Boss => new List<string>
            {
                "Финальный бой с боссом!",
                "Используйте все изученные навыки",
                "Босс очень сильный - будьте осторожны"
            },
            TutorialStage.Completed => new List<string>
            {
                "Поздравляем! Обучение завершено!",
                "Теперь вы готовы к настоящим боям",
                "Удачи в игре!"
            },
            _ => new List<string> { "Следуйте подсказкам на экране" }
        };
    }

    public List<string> GetContextualHints(ITutorialMode session, string gameState)
    {
        return gameState switch
        {
            "player_turn" => new List<string>
            {
                "Ваш ход! Выберите фигуру для хода",
                "Кликните на фигуру, чтобы увидеть возможные ходы"
            },
            "ai_turn" => new List<string>
            {
                "Ход ИИ. Наблюдайте за его стратегией",
                "Изучайте, как ИИ принимает решения"
            },
            "combat" => new List<string>
            {
                "Бой! Выберите цель для атаки",
                "Следите за уроном и здоровьем"
            },
            "ability_use" => new List<string>
            {
                "Используйте способности фигур для преимущества",
                "Каждая способность имеет свою стоимость MP"
            },
            _ => new List<string> { "Следуйте подсказкам на экране" }
        };
    }
}
