using AuthManSys.Domain.Entities;

namespace AuthManSys.Application.Common.Interfaces;

public interface IIdentityExtension
{
    Task<ApplicationUser?> FindByUserNameAsync(string userName);
    Task<bool> IsEmailConfirmedAsync(string userName);
    Task<bool> CheckPasswordAsync(ApplicationUser applicationUser, string password);
    Task<IList<string>> GetUserRolesAsync(ApplicationUser user);


}