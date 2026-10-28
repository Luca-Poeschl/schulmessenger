using Microsoft.EntityFrameworkCore;
using Schulmessenger.Application.Interfaces.Db;
using Schulmessenger.Application.Interfaces.Services;
using Schulmessenger.Application.Models;

namespace Schulmessenger.Application.Services;

public class ClassService(ISchulmessengerDbContext db) : IClassService
{
    public async Task<IReadOnlyCollection<ClassDto>> GetClasses(CancellationToken cancellationToken = default)
    {
        var classes = await db.Class
            .AsNoTracking()
            .Select(c => new ClassDto(c.ClassId, c.ClassName))
            .ToListAsync(cancellationToken);
        
        return classes;
    }
}