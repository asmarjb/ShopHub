using AutoMapper;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Features.Categories.DTOs;
using ECommerce.Application.Features.Categories.Services;
using ECommerce.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ECommerce.Tests;

public class SoftDeleteRestoreTests
{
    [Fact]
    public async Task SoftDeleteCategory_WithActiveProducts_ShouldFail_UnlessReassigned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        using var context = TestDbContextFactory.Create(nameof(SoftDeleteCategory_WithActiveProducts_ShouldFail_UnlessReassigned), userId);

        var category = new Category { Id = Guid.NewGuid(), Name = "Kitablar", Slug = "kitablar" };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Proqramlaşdırma Kitabı",
            Slug = "prog-kitab",
            SKU = "BOOK-01",
            BasePrice = 30m,
            CategoryId = category.Id
        };

        context.Categories.Add(category);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var mockMapper = new Mock<IMapper>();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.UserId).Returns(userId);

        var categoryService = new CategoryService(context, mockMapper.Object, mockCurrentUserService.Object);

        // Act 1: Direct soft delete without reassign
        var directDeleteResult = await categoryService.SoftDeleteAsync(category.Id, null);

        // Assert 1: Restrict should prevent deletion
        directDeleteResult.IsSuccess.Should().BeFalse();
        directDeleteResult.Error.Message.ToLower().Should().Contain("product");

        // Act 2: Create new category and soft delete with reassign
        var targetCategory = new Category { Id = Guid.NewGuid(), Name = "Ümumi", Slug = "umumi" };
        context.Categories.Add(targetCategory);
        await context.SaveChangesAsync();

        var reassignDeleteResult = await categoryService.SoftDeleteAsync(category.Id, targetCategory.Id);

        // Assert 2: Successful reassign and soft delete
        reassignDeleteResult.IsSuccess.Should().BeTrue();

        var updatedProduct = await context.Products.FindAsync(product.Id);
        updatedProduct!.CategoryId.Should().Be(targetCategory.Id);

        var deletedCategory = await context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == category.Id);
        deletedCategory!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task RestoreCategory_ShouldMakeItActiveAgain()
    {
        // Arrange
        var userId = Guid.NewGuid();
        using var context = TestDbContextFactory.Create(nameof(RestoreCategory_ShouldMakeItActiveAgain), userId);

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Paltar",
            Slug = "paltar",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var mockMapper = new Mock<IMapper>();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.UserId).Returns(userId);

        var categoryService = new CategoryService(context, mockMapper.Object, mockCurrentUserService.Object);

        // Act
        var restoreResult = await categoryService.RestoreAsync(category.Id);

        // Assert
        restoreResult.IsSuccess.Should().BeTrue();

        var activeCategory = await context.Categories.FindAsync(category.Id);
        activeCategory.Should().NotBeNull();
        activeCategory!.IsDeleted.Should().BeFalse();
    }
}
