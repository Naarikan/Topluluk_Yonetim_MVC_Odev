using Microsoft.Extensions.Logging;
using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Exceptions;
using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.Enums;
using Topluluk_Yonetim.MVC.Services.Interfaces;

namespace Topluluk_Yonetim.MVC.Services.Implementations;

public class MembershipService : IMembershipService
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IClubService _clubService;
    private readonly ILogger<MembershipService> _logger;

    public MembershipService(
        IMembershipRepository membershipRepository,
        IClubService clubService,
        ILogger<MembershipService> logger)
    {
        _membershipRepository = membershipRepository;
        _clubService = clubService;
        _logger = logger;
    }

    public async Task<IEnumerable<ClubMembership>> GetAllMembershipsAsync()
    {
        return await _membershipRepository.GetAllWithDetailsAsync();
    }

    public async Task<ClubMembership?> GetMembershipByIdAsync(Guid id)
    {
        return await _membershipRepository.GetByIdWithDetailsAsync(id);
    }

    public async Task<IEnumerable<ClubMembership>> GetMembershipsByClubIdAsync(Guid clubId)
    {
        return await _membershipRepository.FindAsync(cm => cm.ClubId == clubId);
    }

    public async Task<IEnumerable<ClubMembership>> GetMembershipsByUserIdAsync(Guid userId)
    {
        return await _membershipRepository.GetByUserIdWithDetailsAsync(userId);
    }

    public async Task<IEnumerable<ClubMembership>> GetPendingMembershipsAsync(Guid? clubId = null)
    {
        if (clubId.HasValue)
            return await _membershipRepository.FindAsync(cm => cm.Status == MembershipStatus.Pending && cm.ClubId == clubId.Value);
        
        return await _membershipRepository.FindAsync(cm => cm.Status == MembershipStatus.Pending);
    }

    public async Task<ClubMembership> RequestMembershipAsync(Guid clubId, Guid userId)
    {
        if (clubId == Guid.Empty)
            throw new ValidationException("Geçersiz topluluk kimliği.");

        if (userId == Guid.Empty)
            throw new ValidationException("Geçersiz kullanıcı kimliği.");

        var club = await _clubService.GetByIdAsync(clubId);
        if (club == null)
            throw new NotFoundException("Topluluk bulunamadı.");

        if (!club.IsActive)
            throw new BusinessRuleException("Bu topluluk şu anda aktif değil.");

        var existingApprovedMembership = await _membershipRepository.FirstOrDefaultAsync(cm =>
            cm.UserId == userId &&
            cm.ClubId == clubId &&
            cm.Status == MembershipStatus.Approved);

        if (existingApprovedMembership != null)
            throw new BusinessRuleException("Bu topluluğa zaten üyesiniz.");

        var existingPendingMembership = await _membershipRepository.FirstOrDefaultAsync(cm =>
            cm.UserId == userId &&
            cm.ClubId == clubId &&
            cm.Status == MembershipStatus.Pending);

        if (existingPendingMembership != null)
            throw new BusinessRuleException("Bu topluluk için zaten bekleyen bir başvurunuz var.");

        var membership = new ClubMembership
        {
            ClubId = clubId,
            UserId = userId,
            Status = MembershipStatus.Pending,
            Role = ClubRole.Member
        };

        await _membershipRepository.AddAsync(membership);
        await _membershipRepository.SaveChangesAsync();

        _logger.LogInformation("Membership request created | User={UserId} | Club={ClubId}", userId, clubId);
        return membership;
    }

    public async Task<bool> ApproveMembershipAsync(Guid membershipId, Guid reviewerId, string? note = null)
    {
        if (membershipId == Guid.Empty)
            throw new ValidationException("Geçersiz üyelik kimliği.");

        var membership = await _membershipRepository.GetByIdAsync(membershipId);
        if (membership == null)
            throw new NotFoundException("Üyelik başvurusu bulunamadı.");

        if (membership.Status != MembershipStatus.Pending)
            throw new BusinessRuleException("Sadece bekleyen başvurular onaylanabilir.");

        membership.Status = MembershipStatus.Approved;
        membership.ReviewedByUserId = reviewerId;
        membership.ReviewedAt = DateTime.UtcNow;
        membership.Note = note;

        _membershipRepository.Update(membership);
        var result = await _membershipRepository.SaveChangesAsync();

        if (result > 0)
        {
            _logger.LogInformation("Membership approved | MembershipId={MembershipId} | Reviewer={ReviewerId} | Note={Note}",
                membership.Id, reviewerId, note);
        }

        return result > 0;
    }

    public async Task<bool> RejectMembershipAsync(Guid membershipId, Guid reviewerId, string? note = null)
    {
        if (membershipId == Guid.Empty)
            throw new ValidationException("Geçersiz üyelik kimliği.");

        var membership = await _membershipRepository.GetByIdAsync(membershipId);
        if (membership == null)
            throw new NotFoundException("Üyelik başvurusu bulunamadı.");

        if (membership.Status != MembershipStatus.Pending)
            throw new BusinessRuleException("Sadece bekleyen başvurular reddedilebilir.");

        membership.Status = MembershipStatus.Rejected;
        membership.ReviewedByUserId = reviewerId;
        membership.ReviewedAt = DateTime.UtcNow;
        membership.Note = note;

        _membershipRepository.Update(membership);
        var result = await _membershipRepository.SaveChangesAsync();

        if (result > 0)
        {
            _logger.LogInformation("Membership rejected | MembershipId={MembershipId} | Reviewer={ReviewerId} | Note={Note}",
                membership.Id, reviewerId, note);
        }

        return result > 0;
    }

    public async Task<bool> CancelMembershipRequestAsync(Guid clubId, Guid userId)
    {
        if (clubId == Guid.Empty)
            throw new ValidationException("Geçersiz topluluk kimliği.");
        
        if (userId == Guid.Empty)
            throw new ValidationException("Geçersiz kullanıcı kimliği.");

        var membership = await _membershipRepository.FirstOrDefaultAsync(cm =>
            cm.ClubId == clubId && 
            cm.UserId == userId && 
            cm.Status == MembershipStatus.Pending);

        if (membership == null)
            throw new BusinessRuleException("Bu topluluk için bekleyen bir başvurunuz bulunmuyor.");

        membership.Status = MembershipStatus.Cancelled;
        _membershipRepository.Update(membership);
        var result = await _membershipRepository.SaveChangesAsync();

        if (result > 0)
        {
            _logger.LogInformation("Membership request cancelled | MembershipId={MembershipId} | User={UserId} | Club={ClubId}",
                membership.Id, userId, clubId);
        }

        return result > 0;
    }

    public async Task<bool> UpdateMembershipRoleAsync(Guid membershipId, ClubRole newRole)
    {
        var membership = await _membershipRepository.GetByIdAsync(membershipId);
        if (membership == null)
            return false;

        membership.Role = newRole;
        _membershipRepository.Update(membership);
        var result = await _membershipRepository.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> RemoveMembershipAsync(Guid membershipId)
    {
        var membership = await _membershipRepository.GetByIdAsync(membershipId);
        if (membership == null)
            return false;

        _membershipRepository.Remove(membership);
        var result = await _membershipRepository.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> IsUserMemberOfClubAsync(Guid userId, Guid clubId)
    {
        return await _membershipRepository.AnyAsync(cm => 
            cm.UserId == userId && 
            cm.ClubId == clubId && 
            cm.Status == MembershipStatus.Approved);
    }

    public async Task<bool> CanUserJoinClubAsync(Guid userId, ClubRole requestedRole)
    {
        if (requestedRole == ClubRole.BoardMember || 
            requestedRole == ClubRole.VicePresident || 
            requestedRole == ClubRole.President)
        {
            var existingMembership = await _membershipRepository.FirstOrDefaultAsync(cm =>
                cm.UserId == userId &&
                (cm.Role == ClubRole.BoardMember ||
                 cm.Role == ClubRole.VicePresident ||
                 cm.Role == ClubRole.President) &&
                cm.Status == MembershipStatus.Approved);

            return existingMembership == null;
        }

        return true;
    }
}

