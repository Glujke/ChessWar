using AutoMapper;
using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Api.Mapping;

/// <summary>
/// Профиль маппинга для боевой системы
/// </summary>
public class AttackMappingProfile : Profile
{
    public AttackMappingProfile()
    {
        CreateMap<AttackApplicationResult, AttackResponseDto>()
            .ForMember(dest => dest.CanAttack, opt => opt.MapFrom(src => src.CanAttack))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason))
            .ForMember(dest => dest.Distance, opt => opt.MapFrom(src => src.Distance))
            .ForMember(dest => dest.MaxRange, opt => opt.MapFrom(src => src.MaxRange));

        CreateMap<AttackRequestDto, Position>()
            .ConstructUsing(src => new Position(src.TargetX, src.TargetY));
    }
}
