using TimeCapsule.Application.DTOs.Capsule;
using TimeCapsule.Domain.Entities;
using TimeCapsule.Domain.Enums;

namespace TimeCapsule.Application.Interfaces;

public interface ICapsuleService
{
    Task<CapsuleResponse> CreateAsync(Guid senderUserId, string senderName, string senderEmail, CreateCapsuleRequest request);
    Task<PagedResponse<CapsuleSummaryResponse>> GetSentAsync(Guid senderUserId, CapsuleStatus? status, int page, int size);
    Task<PagedResponse<CapsuleSummaryResponse>> GetReceivedAsync(Guid recipientUserId, int page, int size);
    Task<CapsuleResponse> GetByIdAsync(Guid capsuleId, Guid requestingUserId, bool isAdmin = false);
    Task<CapsuleResponse> UpdateAsync(Guid capsuleId, Guid requestingUserId, UpdateCapsuleRequest request);
    Task CancelAsync(Guid capsuleId, Guid requestingUserId);
    Task<PagedResponse<CapsuleResponse>> GetAllAdminAsync(CapsuleStatus? status, int page, int size);
    Task<CapsuleStatsResponse> GetStatsAsync();
}

public interface ICapsuleRepository
{
    Task AddAsync(Capsule capsule);
    Task<Capsule?> GetByIdAsync(Guid id);
    Task UpdateAsync(Capsule capsule);
    Task<(IEnumerable<Capsule> Content, long Total)> GetSentAsync(Guid senderUserId, CapsuleStatus? status, int page, int size);
    Task<(IEnumerable<Capsule> Content, long Total)> GetReceivedAsync(Guid recipientUserId, int page, int size);
    Task<List<Capsule>> FindDueForDeliveryAsync(DateTime now, int batchSize);
    Task<(IEnumerable<Capsule> Content, long Total)> GetAllAdminAsync(CapsuleStatus? status, int page, int size);
    Task<CapsuleStatsResponse> GetStatsAsync();
}
