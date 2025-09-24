using AutoMapper;
using ChessWar.Application.DTOs;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Api.Mapping;

/// <summary>
/// Общий профиль маппинга для базовых типов и value objects
/// </summary>
public class CommonMappingProfile : Profile
{
    public CommonMappingProfile()
    {
        CreateMap<Position, PositionDto>()
            .ForMember(dest => dest.X, opt => opt.MapFrom(src => src.X))
            .ForMember(dest => dest.Y, opt => opt.MapFrom(src => src.Y));
    }
}
