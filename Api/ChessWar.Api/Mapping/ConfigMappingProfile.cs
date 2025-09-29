using AutoMapper;
using ChessWar.Application.DTOs;
using ChessWar.Domain.Entities;

namespace ChessWar.Api.Mapping;

/// <summary>
/// Профиль маппинга для конфигурации (баланс, версии)
/// </summary>
public class ConfigMappingProfile : Profile
{
    public ConfigMappingProfile()
    {
        CreateMap<BalanceVersion, ConfigVersionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.PublishedAt, opt => opt.MapFrom(src => src.PublishedAt));

        CreateMap<ChessWar.Persistence.Core.Entities.BalanceVersionDto, ConfigVersionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.PublishedAt, opt => opt.MapFrom(src => src.PublishedAt));

        CreateMap<ChessWar.Persistence.Core.Entities.BalanceVersionDto, object>()
            .ConstructUsing(src => new
            {
                id = src.Id,
                version = src.Version,
                status = src.Status,
                comment = src.Comment,
                createdAt = src.CreatedAt,
                publishedAt = src.PublishedAt
            });
    }
}
