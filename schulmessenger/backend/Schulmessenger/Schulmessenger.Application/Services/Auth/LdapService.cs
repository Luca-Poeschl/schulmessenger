using System.DirectoryServices.Protocols;
using System.Net;
using Schulmessenger.Application.Interfaces.Services.Auth;
using Schulmessenger.Application.Models.Auth;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Services.Auth;

public class LdapService : ILdapService
{
    private readonly string _ldapHost = Environment.GetEnvironmentVariable("LDAP_HOST") ??
                                        throw new InvalidOperationException("LDAP_HOST ist nicht gesetzt!");

    private readonly int _ldapPort = int.Parse(Environment.GetEnvironmentVariable("LDAP_PORT") ??
                                               throw new InvalidOperationException("LDAP_PORT ist nicht gesetzt"));

    private readonly string _ldapDomain = Environment.GetEnvironmentVariable("LDAP_DOMAIN") ??
                                          throw new InvalidOperationException("LDAP_DOMAIN ist nicht gesetzt!");

    private readonly string _ldapBaseDn = Environment.GetEnvironmentVariable("LDAP_BASE_DN") ??
                                          throw new InvalidOperationException("LDAP_BASE_DN ist nicht gesetzt!");

    private readonly string _ldapStudentPath = Environment.GetEnvironmentVariable("LDAP_STUDENT_PATH") ??
                                               throw new InvalidOperationException(
                                                   "LDAP_STUDENT_PATH ist nicht gesetzt!");

    private readonly string _ldapEmployeePath = Environment.GetEnvironmentVariable("LDAP_EMPLOYEE_PATH") ??
                                                throw new InvalidOperationException(
                                                    "LDAP_EMPLOYEE_PATH ist nicht gesetzt!");

    public LdapUserResultDto Authenticate(string username, string password)
    {
        try
        {
            var serverId = new LdapDirectoryIdentifier(_ldapHost, _ldapPort);
            using var connection = new LdapConnection(serverId);
            connection.SessionOptions.ReferralChasing =
                ReferralChasingOptions.None; // Liefert standardmäßig Verweise auf andere Verzeichnisse mit
            connection.SessionOptions.ProtocolVersion = 3;
            connection.Timeout = TimeSpan.FromSeconds(3); // Damit der Thread nicht blockiert wird

            var credential = new NetworkCredential(username, password, _ldapDomain);
            connection.Credential = credential;
            connection.AuthType = AuthType.Basic;
            connection.Bind(credential);

            var searchFilter = $"(sAMAccountName={username})";
            var attributes = new[] { "distinguishedName", "cn" };

            var searchRequest = new SearchRequest(_ldapBaseDn, searchFilter, SearchScope.Subtree, attributes);
            var response = (SearchResponse)connection.SendRequest(searchRequest);

            if (response.Entries.Count == 0)
                return new LdapUserResultDto(false, null, null, null, null);

            var entry = response.Entries[0];
            var dn = entry.DistinguishedName;

            var cn = entry.Attributes["cn"]?[0]?.ToString();
            string firstName;
            string lastName;

            if (!string.IsNullOrWhiteSpace(cn))
            {
                var nameParts = cn.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 2)
                {
                    lastName = nameParts[0];
                    firstName = string.Join(" ", nameParts.Skip(1));
                }
                else
                {
                    lastName = string.Empty;
                    firstName = nameParts[0];
                }
            }
            else
            {
                return new LdapUserResultDto(false, null, null, null, null, "CN wurde nicht gefunden!");
            }

            var (role, className) = ParseRoleAndClassName(dn);

            return role is null
                ? new LdapUserResultDto(false, null, null, null, null, "Benutzer hat keine Rolle!")
                : new LdapUserResultDto(true, firstName, lastName, role, className);
        }
        catch (LdapException le)
        {
            if (le.ErrorCode == 49)
                return new LdapUserResultDto(false, null, null, null, null, "Ungültige Anmeldedaten!");

            return new LdapUserResultDto(false, null, null, null, null, "LDAP-Server nicht erreichbar!");
        }
        catch (Exception)
        {
            return new LdapUserResultDto(false, null, null, null, null, "LDAP-Server nicht erreichbar!");
        }
    }

    private (UserRole? Role, string? ClassName) ParseRoleAndClassName(string dn)
    {
        if (dn.EndsWith(_ldapEmployeePath, StringComparison.OrdinalIgnoreCase))
            return (UserRole.Employee, null);

        if (dn.EndsWith(_ldapStudentPath, StringComparison.OrdinalIgnoreCase))
        {
            var parts = dn.Split(',').Select(p => p.Trim()).ToList();

            var classPart = parts.FirstOrDefault(p =>
                p.StartsWith("OU=", StringComparison.OrdinalIgnoreCase) &&
                !_ldapStudentPath.Contains(p, StringComparison.OrdinalIgnoreCase));

            var className = classPart?.Substring(3);
            return (UserRole.Student, className);
        }

        return (null, null);
    }
}