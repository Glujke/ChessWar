# Инструкция: Создание и публикация конфигурации баланса через Swagger

## Шаги для создания активной версии конфигурации

### 1. Создать черновик версии
- **POST** `/api/v1/config/versions`
- **Body:**
```json
{
  "version": "1.0.0",
  "comment": "Initial default balance from code"
}
```

### 2. Загрузить JSON-конфигурацию
- **PUT** `/api/v1/config/versions/{id}/payload`
- **Body:**
```json
{
  "json": "{\"version\":\"1.0.0\",\"globals\":{\"mpRegenPerTurn\":5,\"cooldownTickPhase\":\"EndTurn\"},\"pieces\":{\"Pawn\":{\"hp\":10,\"atk\":2,\"range\":1,\"movement\":1,\"mp\":5,\"xpToEvolve\":20},\"Knight\":{\"hp\":20,\"atk\":4,\"range\":1,\"movement\":1,\"mp\":8,\"xpToEvolve\":40},\"Bishop\":{\"hp\":18,\"atk\":3,\"range\":4,\"movement\":8,\"mp\":10,\"xpToEvolve\":40},\"Rook\":{\"hp\":25,\"atk\":5,\"range\":8,\"movement\":8,\"mp\":10,\"xpToEvolve\":60},\"Queen\":{\"hp\":30,\"atk\":7,\"range\":3,\"movement\":8,\"mp\":12,\"xpToEvolve\":0},\"King\":{\"hp\":50,\"atk\":3,\"range\":1,\"movement\":1,\"mp\":15,\"xpToEvolve\":0}},\"abilities\":{\"Bishop.LightArrow\":{\"mpCost\":3,\"cooldown\":2,\"range\":4,\"isAoe\":false},\"Bishop.Heal\":{\"mpCost\":6,\"cooldown\":4,\"range\":2,\"isAoe\":false},\"Knight.DoubleStrike\":{\"mpCost\":5,\"cooldown\":3,\"range\":1,\"isAoe\":false},\"Rook.Fortress\":{\"mpCost\":8,\"cooldown\":5,\"range\":0,\"isAoe\":false},\"Queen.MagicExplosion\":{\"mpCost\":10,\"cooldown\":3,\"range\":3,\"isAoe\":true},\"Queen.Resurrection\":{\"mpCost\":12,\"cooldown\":10,\"range\":3,\"isAoe\":false},\"King.RoyalCommand\":{\"mpCost\":10,\"cooldown\":6,\"range\":8,\"isAoe\":false},\"Pawn.ShieldBash\":{\"mpCost\":2,\"cooldown\":0,\"range\":1,\"isAoe\":false},\"Pawn.Breakthrough\":{\"mpCost\":2,\"cooldown\":0,\"range\":1,\"isAoe\":false}},\"evolution\":{\"xpThresholds\":{\"Pawn\":20,\"Knight\":40,\"Bishop\":40,\"Rook\":60,\"Queen\":0,\"King\":0},\"rules\":{\"Pawn\":[\"Knight\",\"Bishop\"],\"Knight\":[\"Rook\"],\"Bishop\":[\"Rook\"],\"Rook\":[\"Queen\"],\"Queen\":[],\"King\":[]},\"immediateOnLastRank\":{\"Pawn\":true}},\"ai\":{\"nearEvolutionXp\":19,\"lastRankEdgeY\":{\"Elves\":6,\"Orcs\":1},\"kingAura\":{\"radius\":3,\"atkBonus\":1}}}"
}
```

### 3. Опубликовать версию (сделать активной)
- **POST** `/api/v1/config/versions/{id}/publish`

### 4. Проверить активную конфигурацию
- **GET** `/api/v1/config/active`

## Готовый JSON для копирования

Полный JSON конфигурации находится в файле `Api/ChessWar.Api/Examples/DefaultBalanceConfig.json` - скопируйте его содержимое в поле `json` при загрузке payload.

## Примечания

- Только версии со статусом "Draft" можно редактировать
- При публикации новой версии предыдущая автоматически переводится в статус "Published"
- Активная версия используется всеми игровыми сервисами через `IBalanceConfigProvider`
