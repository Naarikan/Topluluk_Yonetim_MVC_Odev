using Microsoft.Extensions.Logging;
using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Exceptions;
using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.Enums;
using Topluluk_Yonetim.MVC.Services.Interfaces;

namespace Topluluk_Yonetim.MVC.Services.Implementations;

public class ClubApplicationService : Service<ClubApplication>, IClubApplicationService
{
    private readonly IClubApplicationRepository _applicationRepository;
    private readonly IRepository<Club> _clubRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IAuthService _authService;
    private readonly ILogger<ClubApplicationService> _logger;

    public ClubApplicationService(
        IClubApplicationRepository applicationRepository,
        IRepository<Club> clubRepository,
        IMembershipRepository membershipRepository,
        IAuthService authService,
        ILogger<ClubApplicationService> logger) : base(applicationRepository)
    {
        _applicationRepository = applicationRepository;
        _clubRepository = clubRepository;
        _membershipRepository = membershipRepository;
        _authService = authService;
        _logger = logger;
    }

    public override async Task<ClubApplication> CreateAsync(ClubApplication entity)
    {

        var existingApplication = await _applicationRepository.FirstOrDefaultAsync(a =>
            a.ClubName.ToLower() == entity.ClubName.ToLower() &&
            (a.Status == ApprovalStatus.Pending || a.Status == ApprovalStatus.Approved));

        if (existingApplication != null)
            throw new BusinessRuleException($"'{entity.ClubName}' adında zaten bir topluluk başvurusu mevcut.");

        var existingClub = await _clubRepository.FirstOrDefaultAsync(c =>
            c.Name.ToLower() == entity.ClubName.ToLower() && c.IsActive);

        if (existingClub != null)
            throw new BusinessRuleException($"'{entity.ClubName}' adında zaten aktif bir topluluk mevcut.");

        return await base.CreateAsync(entity);
    }

    public async Task<IEnumerable<ClubApplication>> GetAllWithDetailsAsync()
    {
        return await _applicationRepository.GetAllWithDetailsAsync();
    }

    public async Task<ClubApplication?> GetByIdWithDetailsAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ValidationException("Geçersiz başvuru kimliği.");

        return await _applicationRepository.GetByIdWithDetailsAsync(id);
    }

    public async Task<IEnumerable<ClubApplication>> GetPendingApplicationsAsync()
    {
        return await _applicationRepository.FindAsync(a => a.Status == ApprovalStatus.Pending);
    }

    public async Task<bool> ApproveApplicationAsync(Guid applicationId, Guid reviewerId, string? note = null)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId);
        if (application == null)
            throw new NotFoundException("Topluluk başvurusu bulunamadı.");

        if (application.Status != ApprovalStatus.Pending)
            throw new BusinessRuleException($"Bu başvuru zaten {GetStatusText(application.Status)} durumunda.");

        var existingPresidentClub = await _clubRepository.FirstOrDefaultAsync(c =>
            c.PresidentId == application.ApplicantUserId && c.IsActive);
        if (existingPresidentClub != null)
            throw new BusinessRuleException("Başvuru sahibi başka bir kulüpte başkan. Tek kulüp başkanlığına izin verilir.");

        var existingClub = await _clubRepository.FirstOrDefaultAsync(c =>
            c.Name.ToLower() == application.ClubName.ToLower() && c.IsActive);

        if (existingClub != null)
            throw new BusinessRuleException($"'{application.ClubName}' adında zaten aktif bir topluluk mevcut. Başvuru onaylanamaz.");

        application.Status = ApprovalStatus.Approved;
        application.ReviewedByUserId = reviewerId;
        application.ReviewedAt = DateTime.UtcNow;
        application.CoordinatorNote = note;

        var club = new Club
        {
            Name = application.ClubName,
            Description = application.Mission,
            AdvisorName = application.AdvisorName,
            PresidentId = application.ApplicantUserId,
            IsActive = true
        };

        await _clubRepository.AddAsync(club);

        var presidentMembership = new ClubMembership
        {
            ClubId = club.Id,
            UserId = application.ApplicantUserId,
            Status = MembershipStatus.Approved,
            Role = ClubRole.President,
            ReviewedByUserId = reviewerId,
            ReviewedAt = DateTime.UtcNow
        };
        await _membershipRepository.AddAsync(presidentMembership);

        _applicationRepository.Update(application);
        await _applicationRepository.SaveChangesAsync();
        await _membershipRepository.SaveChangesAsync();

        await _authService.AssignRoleAsync(application.ApplicantUserId, "President");
        await _authService.AssignRoleAsync(application.ApplicantUserId, "Member");

        _logger.LogInformation(
            "Club application approved | ApplicationId={ApplicationId} | ClubId={ClubId} | PresidentUserId={PresidentUserId} | Reviewer={ReviewerId}",
            application.Id, club.Id, application.ApplicantUserId, reviewerId);

        return true;
    }

    public async Task<bool> RejectApplicationAsync(Guid applicationId, Guid reviewerId, string? note = null)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId);
        if (application == null)
            throw new NotFoundException("Topluluk başvurusu bulunamadı.");

        if (application.Status != ApprovalStatus.Pending)
            throw new BusinessRuleException($"Bu başvuru zaten {GetStatusText(application.Status)} durumunda.");

        application.Status = ApprovalStatus.Rejected;
        application.ReviewedByUserId = reviewerId;
        application.ReviewedAt = DateTime.UtcNow;
        application.CoordinatorNote = note;

        _applicationRepository.Update(application);
        var result = await _applicationRepository.SaveChangesAsync();
        return result > 0;
    }

    public async Task<IEnumerable<ClubApplication>> GetApplicationsByUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("Geçersiz kullanıcı kimliği.");

        return await _applicationRepository.FindAsync(a => a.ApplicantUserId == userId);
    }

    private static string GetStatusText(ApprovalStatus status)
    {
        return status switch
        {
            ApprovalStatus.Pending => "beklemede",
            ApprovalStatus.Approved => "onaylanmış",
            ApprovalStatus.Rejected => "reddedilmiş",
            _ => "bilinmeyen"
        };
    }

}

