using Microsoft.EntityFrameworkCore;
using TimeCapsule.Application.DTOs.Capsule;
using TimeCapsule.Application.Interfaces;
using TimeCapsule.Domain.Entities;
using TimeCapsule.Domain.Enums;
using TimeCapsule.Infrastructure.Data;

namespace TimeCapsule.Infrastructure.Repositories;

public class CapsuleRepository : ICapsuleRepository
{
    private readonly AppDbContext _db;

    public CapsuleRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Capsule capsule)
    {
        await _db.Capsules.AddAsync(capsule);
        await _db.SaveChangesAsync();
    }

    public async Task<Capsule?> GetByIdAsync(Guid id)
    {
        return await _db.Capsules.FindAsync(id);
    }

    public async Task UpdateAsync(Capsule capsule)
    {
        _db.Capsules.Update(capsule);
        await _db.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Capsule> Content, long Total)> GetSentAsync(Guid senderUserId, CapsuleStatus? status, int page, int size)
    {
        var query = _db.Capsules.Where(c => c.SenderUserId == senderUserId);
        
        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        var total = await query.LongCountAsync();
        var content = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        return (content, total);
    }

    public async Task<(IEnumerable<Capsule> Content, long Total)> GetReceivedAsync(Guid recipientUserId, int page, int size)
    {
        var query = _db.Capsules.Where(c => c.RecipientUserId == recipientUserId && c.Status == CapsuleStatus.Sent);
        
        var total = await query.LongCountAsync();
        var content = await query
            .OrderByDescending(c => c.SentAt)
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        return (content, total);
    }

    public async Task<List<Capsule>> FindDueForDeliveryAsync(DateTime now, int batchSize)
    {
        return await _db.Capsules
            .Where(c => c.DeliverAt <= now && c.Status == CapsuleStatus.Scheduled)
            .OrderBy(c => c.DeliverAt)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Capsule> Content, long Total)> GetAllAdminAsync(CapsuleStatus? status, int page, int size)
    {
        var query = _db.Capsules.AsQueryable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        var total = await query.LongCountAsync();
        var content = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        return (content, total);
    }

    public async Task<CapsuleStatsResponse> GetStatsAsync()
    {
        var total = await _db.Capsules.LongCountAsync();
        var scheduled = await _db.Capsules.LongCountAsync(c => c.Status == CapsuleStatus.Scheduled);
        var sent = await _db.Capsules.LongCountAsync(c => c.Status == CapsuleStatus.Sent);
        var cancelled = await _db.Capsules.LongCountAsync(c => c.Status == CapsuleStatus.Cancelled);
        var failed = await _db.Capsules.LongCountAsync(c => c.Status == CapsuleStatus.Failed);

        return new CapsuleStatsResponse(total, scheduled, sent, cancelled, failed);
    }
}
