using Schulmessenger.Application.Models.Auth;

namespace Schulmessenger.Application.Interfaces.Services.Auth;

public interface ILdapService
{
    LdapUserResultDto Authenticate(string username, string password);
}