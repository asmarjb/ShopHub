using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ECommerce.Tests;

public static class TestDbContextFactory
{
    public static AppDbContext Create(string dbName, Guid? currentUserId = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.UserId).Returns(currentUserId ?? Guid.NewGuid());
        mockCurrentUserService.Setup(s => s.UserName).Returns("admin@test.com");
        mockCurrentUserService.Setup(s => s.IpAddress).Returns("127.0.0.1");

        var context = new AppDbContext(options, mockCurrentUserService.Object);
        return context;
    }
}
