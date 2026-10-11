using AutoMapper;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Shipping.DTOs;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Shipping.Services;

public class ShippingService : IShippingService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ShippingService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<ShippingMethodDto>>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.ShippingMethods.AsNoTracking();
        if (!includeInactive)
            query = query.Where(sm => sm.IsActive);

        var methods = await query.OrderBy(sm => sm.Price).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<ShippingMethodDto>>(_mapper.Map<List<ShippingMethodDto>>(methods));
    }

    public async Task<Result<ShippingMethodDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var method = await _context.ShippingMethods.AsNoTracking().FirstOrDefaultAsync(sm => sm.Id == id, cancellationToken);
        if (method == null)
            return Result.Failure<ShippingMethodDto>(Error.NotFound("ShippingMethod", id));

        return Result.Success(_mapper.Map<ShippingMethodDto>(method));
    }

    public async Task<Result<ShippingMethodDto>> CreateAsync(CreateShippingMethodDto dto, CancellationToken cancellationToken = default)
    {
        var method = new ShippingMethod
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            EstimatedDeliveryDays = dto.EstimatedDeliveryDays,
            IsActive = dto.IsActive
        };

        _context.ShippingMethods.Add(method);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<ShippingMethodDto>(method));
    }

    public async Task<Result<ShippingMethodDto>> UpdateAsync(Guid id, UpdateShippingMethodDto dto, CancellationToken cancellationToken = default)
    {
        var method = await _context.ShippingMethods.FirstOrDefaultAsync(sm => sm.Id == id, cancellationToken);
        if (method == null)
            return Result.Failure<ShippingMethodDto>(Error.NotFound("ShippingMethod", id));

        method.Name = dto.Name;
        method.Description = dto.Description;
        method.Price = dto.Price;
        method.EstimatedDeliveryDays = dto.EstimatedDeliveryDays;
        method.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(_mapper.Map<ShippingMethodDto>(method));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var method = await _context.ShippingMethods
            .Include(sm => sm.Orders)
            .FirstOrDefaultAsync(sm => sm.Id == id, cancellationToken);

        if (method == null)
            return Result.Failure(Error.NotFound("ShippingMethod", id));

        if (method.Orders.Any())
            return Result.Failure("Cannot delete shipping method that has linked historical orders. Deactivate it instead.");

        _context.ShippingMethods.Remove(method);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
