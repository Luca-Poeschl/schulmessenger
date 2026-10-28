using Microsoft.EntityFrameworkCore;
using Schulmessenger.Application.Interfaces.Db;
using Schulmessenger.Application.Interfaces.Services.Auth;
using Schulmessenger.Application.Models.Auth;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Services.Auth;

public class AuthService(ILdapService ldapService, ITokenService tokenService, ISchulmessengerDbContext db)
    : IAuthService
{
    public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto,
        CancellationToken cancellationToken = default)
    {
        var ldapResult = ldapService.Authenticate(loginRequestDto.Username, loginRequestDto.Password);

        if (!ldapResult.Success || ldapResult.Role is null)
            throw new InvalidOperationException(ldapResult.ErrorMessage ?? "Anmeldung fehlgeschlagen!");

        var user = await db.User
            .Include(u => u.Student)
            .ThenInclude(s => s!.Class)
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u =>
                u.Username == loginRequestDto.Username, cancellationToken);

        if (user is null)
        {
            user = new User()
            {
                UserId = Guid.NewGuid(),
                Username = loginRequestDto.Username,
                FirstName = ldapResult.FirstName!,
                LastName = ldapResult.LastName!,
                Role = ldapResult.Role.Value,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            if (ldapResult.Role == UserRole.Employee)
            {
                user.Employee = new Employee() { UserId = user.UserId };
            }
            else if (ldapResult.Role == UserRole.Student)
            {
                user.Student = new Student()
                {
                    UserId = user.UserId,
                };

                if (!string.IsNullOrEmpty(ldapResult.ClassName))
                {
                    var studentClass = await db.Class.FirstOrDefaultAsync(c => c.ClassName == ldapResult.ClassName, cancellationToken);

                    if (studentClass is null)
                        studentClass = new Class() { ClassId = Guid.NewGuid(), ClassName = ldapResult.ClassName! };

                    user.Student.Class = studentClass;
                }
            }

            db.User.Add(user);
        }
        else
        {
            user.FirstName = ldapResult.FirstName ?? user.FirstName;
            user.LastName = ldapResult.LastName ?? user.LastName;
            user.LastLoginAt = DateTime.UtcNow;

            if (user.Role == UserRole.Student)
            {
                if (user.Student is null)
                    user.Student = new Student() { UserId = user.UserId };

                if (!string.IsNullOrEmpty(ldapResult.ClassName))
                {
                    var userClass = await db.Class.FirstOrDefaultAsync(c => c.ClassName == ldapResult.ClassName,
                        cancellationToken);

                    if (userClass is null)
                        userClass = new Class() { ClassId = Guid.NewGuid(), ClassName = ldapResult.ClassName };

                    user.Student.Class = userClass;
                }
            }
        }

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        var refreshTokenObject = new RefreshToken()
        {
            Token = refreshToken,
            UserId = user.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        db.RefreshToken.Add(refreshTokenObject);

        var changedLines = await db.SaveChangesAsync(cancellationToken);
        if (changedLines == 0)
            throw new Exception("Unerwarteter Fehler beim Speichern des Users!");

        return new LoginResponseDto(accessToken, refreshToken, user.UserId, user.FirstName, user.LastName, user.Role,
            user.Student?.Class?.ClassName);
    }

    public async Task<TokenResultDto?> RefreshTokens(string refreshToken, CancellationToken cancellationToken = default)
    {
        var existingToken = await db.RefreshToken
            .Include(r => r.User)
            .ThenInclude(u => u.Student)
            .ThenInclude(s => s!.Class)
            .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

        if (existingToken is null || existingToken.ExpiresAt <= DateTime.UtcNow)
            return null;

        db.RefreshToken.Remove(existingToken);

        var newAccessToken = tokenService.GenerateAccessToken(existingToken.User);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        db.RefreshToken.Add(
            new RefreshToken()
            {
                Token = newRefreshToken,
                UserId = existingToken.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

        var changedLines = await db.SaveChangesAsync(cancellationToken);
        if (changedLines == 0)
            throw new Exception("Unerwarteter Fehler beim Refreshen der Tokens!");

        return new TokenResultDto(newAccessToken, newRefreshToken);
    }

    public async Task<bool> RevokeToken(string refreshToken, CancellationToken cancellationToken = default)
    {
        var existingToken = await db.RefreshToken
            .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

        if (existingToken is null)
            return false;

        db.RefreshToken.Remove(existingToken);

        var changedLines = await db.SaveChangesAsync(cancellationToken);
        if (changedLines == 0)
            throw new Exception("Unerwarteter Fehler beim Löschen des Tokens!");

        return true;
    }
}