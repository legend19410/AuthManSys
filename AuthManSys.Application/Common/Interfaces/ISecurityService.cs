
namespace AuthManSys.Application.Common.Interfaces;

public interface ISecurityService
{
    string GenerateToken(string username, string email);

}
