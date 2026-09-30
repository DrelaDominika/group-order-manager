using GroupOrderManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GroupOrderManager.Tests;

internal static class TestDb
{
    // A fresh, isolated in-memory database for each test, so no data leaks between tests.
    public static GomDbContext Create()
    {
        var options = new DbContextOptionsBuilder<GomDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GomDbContext(options);
    }
}
