using AutoMapper;
using Microsoft.Extensions.Configuration;
using TimeCapsule.Application.DTOs.Capsule;
using TimeCapsule.Application.Interfaces;
using TimeCapsule.Domain.Entities;
using TimeCapsule.Domain.Enums;
using TimeCapsule.Domain.Exceptions;

namespace TimeCapsule.Application.Services;

public class CapsuleService : ICapsuleService
{
    private readonly ICapsuleRepository _capsuleRepo;
    private readonly IUserRepository _userRepo;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;

    public CapsuleService(
        ICapsuleRepository capsuleRepo,
        IUserRepository userRepo,
        IMapper mapper,
        IConfiguration config)
    {
        _capsuleRepo = capsuleRepo;
        _userRepo = userRepo;
        _mapper = mapper;
        _config = config;
    }

    public async Task<CapsuleResponse> CreateAsync(
        Guid senderUserId, string senderName, string senderEmail,
        CreateCapsuleRequest request)
    {
        if (request.DeliverAt <= DateTime.UtcNow)
            throw new AppException("invalid_deliver_at",
                "Delivery time must be in the future.", 400);

        Guid? recipientUserId = null;
        string recipientName = request.RecipientName ?? request.RecipientEmail ?? "Guest";

        if (!string.IsNullOrEmpty(request.RecipientEmail))
        {
            var recipient = await _userRepo.FindByEmailAsync(request.RecipientEmail);
            if (recipient is not null)
            {
                recipientUserId = recipient.Id;
                recipientName = recipient.Name;
            }
        }

        var capsule = new Capsule
        {
            SenderUserId = senderUserId,
            SenderName = senderName,
            SenderEmail = senderEmail,
            RecipientUserId = recipientUserId,
            RecipientName = recipientName,
            RecipientEmail = request.RecipientEmail,
            RecipientPhone = request.RecipientPhone,
            Subject = request.Subject,
            Message = request.Message,
            DeliveryChannel = request.DeliveryChannel,
            DeliverAt = request.DeliverAt,
            Status = CapsuleStatus.Scheduled
        };

        await _capsuleRepo.AddAsync(capsule);
        return _mapper.Map<CapsuleResponse>(capsule);
    }

    public async Task<PagedResponse<CapsuleSummaryResponse>> GetSentAsync(Guid senderUserId, CapsuleStatus? status, int page, int size)
    {
        var (content, total) = await _capsuleRepo.GetSentAsync(senderUserId, status, page, size);
        var dtos = _mapper.Map<IEnumerable<CapsuleSummaryResponse>>(content);
        return new PagedResponse<CapsuleSummaryResponse>(
            dtos, page, size, total, (int)Math.Ceiling(total / (double)size));
    }

    public async Task<PagedResponse<CapsuleSummaryResponse>> GetReceivedAsync(Guid recipientUserId, int page, int size)
    {
        var (content, total) = await _capsuleRepo.GetReceivedAsync(recipientUserId, page, size);
        var dtos = _mapper.Map<IEnumerable<CapsuleSummaryResponse>>(content);
        return new PagedResponse<CapsuleSummaryResponse>(
            dtos, page, size, total, (int)Math.Ceiling(total / (double)size));
    }

    public async Task<CapsuleResponse> GetByIdAsync(Guid capsuleId, Guid requestingUserId, bool isAdmin = false)
    {
        var capsule = await _capsuleRepo.GetByIdAsync(capsuleId)
            ?? throw new NotFoundException("capsule_not_found", "Capsule not found.");

        if (isAdmin)
        {
            return _mapper.Map<CapsuleResponse>(capsule);
        }

        bool isSender = capsule.SenderUserId == requestingUserId;
        bool isRecipient = capsule.RecipientUserId == requestingUserId;

        if (!isSender && !isRecipient)
            throw new UnauthorizedException("forbidden", "Access denied.");

        if (isRecipient && !isSender && capsule.Status != CapsuleStatus.Sent)
            throw new UnauthorizedException("capsule_sealed", "This capsule has not been delivered yet.");

        return _mapper.Map<CapsuleResponse>(capsule);
    }

    public async Task<CapsuleResponse> UpdateAsync(Guid capsuleId, Guid requestingUserId, UpdateCapsuleRequest request)
    {
        var capsule = await _capsuleRepo.GetByIdAsync(capsuleId)
            ?? throw new NotFoundException("capsule_not_found", "Capsule not found.");

        if (capsule.SenderUserId != requestingUserId)
            throw new UnauthorizedException("forbidden", "Access denied.");

        if (capsule.Status != CapsuleStatus.Scheduled)
            throw new AppException("capsule_not_scheduled", "Only scheduled capsules can be updated.", 400);

        capsule.Subject = request.Subject;
        capsule.Message = request.Message;
        capsule.DeliverAt = request.DeliverAt;
        capsule.DeliveryChannel = request.DeliveryChannel;
        capsule.RecipientEmail = request.RecipientEmail;
        capsule.RecipientPhone = request.RecipientPhone;
        capsule.RecipientName = request.RecipientName ?? request.RecipientEmail ?? "Guest";
        capsule.UpdatedAt = DateTime.UtcNow;

        await _capsuleRepo.UpdateAsync(capsule);
        return _mapper.Map<CapsuleResponse>(capsule);
    }

    public async Task CancelAsync(Guid capsuleId, Guid requestingUserId)
    {
        var capsule = await _capsuleRepo.GetByIdAsync(capsuleId)
            ?? throw new NotFoundException("capsule_not_found", "Capsule not found.");

        if (capsule.SenderUserId != requestingUserId)
            throw new UnauthorizedException("forbidden", "Access denied.");

        if (capsule.Status != CapsuleStatus.Scheduled)
            throw new AppException("capsule_not_scheduled", "Only scheduled capsules can be cancelled.", 400);

        capsule.Status = CapsuleStatus.Cancelled;
        capsule.UpdatedAt = DateTime.UtcNow;

        await _capsuleRepo.UpdateAsync(capsule);
    }

    public async Task<PagedResponse<CapsuleResponse>> GetAllAdminAsync(CapsuleStatus? status, int page, int size)
    {
        var (content, total) = await _capsuleRepo.GetAllAdminAsync(status, page, size);
        var dtos = _mapper.Map<IEnumerable<CapsuleResponse>>(content);
        return new PagedResponse<CapsuleResponse>(
            dtos, page, size, total, (int)Math.Ceiling(total / (double)size));
    }

    public async Task<CapsuleStatsResponse> GetStatsAsync()
    {
        return await _capsuleRepo.GetStatsAsync();
    }
}
