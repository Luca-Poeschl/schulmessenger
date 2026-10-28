using Schulmessenger.Application.Models;

namespace Schulmessenger.Application.Interfaces.Services;

public interface IClassService
{
    public Task<IReadOnlyCollection<ClassDto>> GetClasses(CancellationToken cancellationToken = default);
}