using Microsoft.EntityFrameworkCore;
using PhotoPipeline.Database.Model;

namespace PhotoPipeline.Database.MsSql;

public class MsSqlPhotoDbContext : PhotoDbContext
{
    public MsSqlPhotoDbContext(DbContextOptions<PhotoDbContext> options) : base(options)
    {
    }
}
