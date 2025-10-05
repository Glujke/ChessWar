using AutoMapper;
using ChessWar.Application.DTOs;
using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Api.Mapping;

/// <summary>
/// Профиль маппинга для игровой логики (фигуры, доска, ходы)
/// </summary>
public class GameMappingProfile : Profile
{
    public GameMappingProfile()
    {
        CreateMap<Domain.Entities.Piece, ChessWar.Application.DTOs.PieceDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Team, opt => opt.MapFrom(src => src.Team))
            .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.IsAlive ? new PositionDto { X = src.Position.X, Y = src.Position.Y } : null))
            .ForMember(dest => dest.HP, opt => opt.MapFrom(src => src.HP))
            .ForMember(dest => dest.ATK, opt => opt.MapFrom(src => src.ATK))
            .ForMember(dest => dest.Range, opt => opt.MapFrom(src => src.Range))
            .ForMember(dest => dest.Movement, opt => opt.MapFrom(src => src.Movement))
            .ForMember(dest => dest.XP, opt => opt.MapFrom(src => src.XP))
            .ForMember(dest => dest.XPToEvolve, opt => opt.MapFrom(src => src.XPToEvolve))
            .ForMember(dest => dest.IsAlive, opt => opt.MapFrom(src => src.IsAlive))
            .ForMember(dest => dest.IsFirstMove, opt => opt.MapFrom(src => src.IsFirstMove))
            .ForMember(dest => dest.AbilityCooldowns, opt => opt.MapFrom(src => src.AbilityCooldowns))
            .ForMember(dest => dest.ShieldHp, opt => opt.MapFrom(src => src.ShieldHP))
            .ForMember(dest => dest.NeighborCount, opt => opt.MapFrom(src => src.NeighborCount));

        CreateMap<GameBoard, GameBoardDto>()
            .ForMember(dest => dest.Pieces, opt => opt.MapFrom(src => src.Pieces))
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => GameBoard.Size));

        CreateMap<Domain.Entities.GameSession, ChessWar.Application.DTOs.GameSessionDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.Mode, opt => opt.MapFrom(src => src.Mode))
            .ForMember(dest => dest.Player1, opt => opt.MapFrom(src => src.Player1))
            .ForMember(dest => dest.Player2, opt => opt.MapFrom(src => src.Player2))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Result, opt => opt.MapFrom(src => src.Result))
            .ForMember(dest => dest.CurrentTurn, opt => opt.MapFrom(src => src.CurrentTurn));

        CreateMap<Domain.Entities.TutorialSession, ChessWar.Application.DTOs.TutorialSessionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.Mode, opt => opt.MapFrom(src => src.Mode))
            .ForMember(dest => dest.CurrentScenario, opt => opt.MapFrom(src => src.CurrentScenario))
            .ForMember(dest => dest.CurrentStage, opt => opt.MapFrom(src => src.CurrentStage))
            .ForMember(dest => dest.Progress, opt => opt.MapFrom(src => src.Progress))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.IsCompleted))
            .ForMember(dest => dest.ShowHints, opt => opt.MapFrom(src => src.ShowHints))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.SignalRUrl, opt => opt.MapFrom(src => $"/gameHub?sessionId={src.Id}"))
            .ForMember(dest => dest.Scenario, opt => opt.MapFrom(src => new TutorialScenarioDto
            {
                Type = src.CurrentScenario.ToString(),
                Difficulty = "Easy"
            }))
            .ForMember(dest => dest.Board, opt => opt.MapFrom(src => new TutorialBoardDto
            {
                Width = GameBoard.Size,
                Height = GameBoard.Size
            }))
            .ForMember(dest => dest.Pieces, opt => opt.Ignore());

        CreateMap<Domain.Entities.Player, ChessWar.Application.DTOs.PlayerDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.Pieces, opt => opt.MapFrom(src => src.Pieces))
            .ForMember(dest => dest.MP, opt => opt.MapFrom(src => src.MP))
            .ForMember(dest => dest.MaxMP, opt => opt.MapFrom(src => src.MaxMP));

        CreateMap<Domain.Entities.AI, ChessWar.Application.DTOs.PlayerDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.Pieces, opt => opt.MapFrom(src => src.Pieces))
            .ForMember(dest => dest.MP, opt => opt.MapFrom(src => src.MP))
            .ForMember(dest => dest.MaxMP, opt => opt.MapFrom(src => src.MaxMP));

        CreateMap<Domain.Entities.Participant, ChessWar.Application.DTOs.PlayerDto>()
            .Include<Domain.Entities.Player, ChessWar.Application.DTOs.PlayerDto>()
            .Include<Domain.Entities.AI, ChessWar.Application.DTOs.PlayerDto>();

        CreateMap<Domain.ValueObjects.Turn, ChessWar.Application.DTOs.TurnDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.SelectedPiece, opt => opt.MapFrom(src => src.SelectedPiece));

        CreateMap<Domain.ValueObjects.TurnAction, ChessWar.Application.DTOs.TurnActionDto>()
            .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => src.ActionType))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp));

        CreateMap<CreatePieceDto, Position>()
            .ConstructUsing(src => new Position(src.X, src.Y));

        CreateMap<PlacePieceDto, Position>()
            .ConstructUsing(src => new Position(src.X, src.Y));

        CreateMap<UpdatePieceDto, Position>()
            .ConstructUsing(src => new Position(src.X ?? 0, src.Y ?? 0));
    }
}
