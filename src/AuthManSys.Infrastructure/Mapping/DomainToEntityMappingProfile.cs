using AutoMapper;
using DomainEntities = AuthManSys.Domain.Entities;
using EfEntities = AuthManSys.Infrastructure.Database.Entities;

namespace AuthManSys.Infrastructure.Mapping;

public class DomainToEntityMappingProfile : Profile
{
    public DomainToEntityMappingProfile()
    {
        // User mappings
        CreateMap<DomainEntities.User, EfEntities.ApplicationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Identity will handle this
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TermsConditionsAccepted, opt => opt.MapFrom(src => src.TermsAccepted))
            .ForMember(dest => dest.EmailConfirmationToken, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordResetToken, opt => opt.Ignore())
            .ForMember(dest => dest.RequestVerificationToken, opt => opt.Ignore())
            .ForMember(dest => dest.RequestVerificationTokenExpiration, opt => opt.Ignore())
            .ForMember(dest => dest.LastPasswordChangedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorCode, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorCodeExpiration, opt => opt.Ignore())
            .ForMember(dest => dest.IsTwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorCodeGeneratedAt, opt => opt.Ignore())
            .ForMember(dest => dest.GoogleId, opt => opt.Ignore())
            .ForMember(dest => dest.GoogleEmail, opt => opt.Ignore())
            .ForMember(dest => dest.GooglePictureUrl, opt => opt.Ignore())
            .ForMember(dest => dest.IsGoogleAccount, opt => opt.Ignore())
            .ForMember(dest => dest.GoogleLinkedAt, opt => opt.Ignore());

        CreateMap<EfEntities.ApplicationUser, DomainEntities.User>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<Domain.Enums.UserStatus>(src.Status)))
            .ForMember(dest => dest.TermsAccepted, opt => opt.MapFrom(src => src.TermsConditionsAccepted))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()); // Will be set from other source if needed

        // Role mappings
        CreateMap<DomainEntities.Role, EfEntities.ApplicationRole>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.NormalizedName, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore());

        CreateMap<EfEntities.ApplicationRole, DomainEntities.Role>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)));

        // Permission mappings
        CreateMap<DomainEntities.Permission, EfEntities.Permission>();
        CreateMap<EfEntities.Permission, DomainEntities.Permission>()
            .ForMember(dest => dest.RolePermissions, opt => opt.Ignore()); // Ignore navigation property for now

        // RefreshToken mappings
        CreateMap<DomainEntities.RefreshToken, EfEntities.RefreshToken>()
            .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpiresAt))
            .ForMember(dest => dest.Used, opt => opt.MapFrom(src => src.IsUsed))
            .ForMember(dest => dest.Invalidated, opt => opt.MapFrom(src => src.IsInvalidated));

        CreateMap<EfEntities.RefreshToken, DomainEntities.RefreshToken>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreationDate))
            .ForMember(dest => dest.ExpiresAt, opt => opt.MapFrom(src => src.ExpirationDate))
            .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.Used))
            .ForMember(dest => dest.IsInvalidated, opt => opt.MapFrom(src => src.Invalidated));

        // RolePermission mappings
        CreateMap<DomainEntities.RolePermission, EfEntities.RolePermission>()
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId.ToString()));

        CreateMap<EfEntities.RolePermission, DomainEntities.RolePermission>()
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => Guid.Parse(src.RoleId)));

        // UserRole mappings
        CreateMap<DomainEntities.UserRole, EfEntities.ApplicationUserRole>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()))
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId.ToString()));

        CreateMap<EfEntities.ApplicationUserRole, DomainEntities.UserRole>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => int.Parse(src.UserId)))
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => Guid.Parse(src.RoleId)));

        // UserActivityLog mappings
        CreateMap<DomainEntities.UserActivityLog, EfEntities.UserActivityLog>();
        CreateMap<EfEntities.UserActivityLog, DomainEntities.UserActivityLog>();
    }
}