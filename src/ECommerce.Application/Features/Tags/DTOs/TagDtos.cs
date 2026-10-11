namespace ECommerce.Application.Features.Tags.DTOs;

public record TagDto(
    Guid Id,
    string Name,
    string Slug,
    int ProductCount);

public record CreateTagDto(
    string Name);
