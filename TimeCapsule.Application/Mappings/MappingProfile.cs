using AutoMapper;
using TimeCapsule.Application.DTOs.Auth;
using TimeCapsule.Application.DTOs.Capsule;
using TimeCapsule.Domain.Entities;

namespace TimeCapsule.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserSummary>();
        CreateMap<Capsule, CapsuleResponse>();
        CreateMap<Capsule, CapsuleSummaryResponse>();
    }
}
