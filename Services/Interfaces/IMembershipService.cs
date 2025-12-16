using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Services.Interfaces;

public interface IMembershipService
{
    Task<IEnumerable<ClubMembership>> GetAllMembershipsAsync();
    Task<ClubMembership?> GetMembershipByIdAsync(Guid id);
    Task<IEnumerable<ClubMembership>> GetMembershipsByClubIdAsync(Guid clubId);
    Task<IEnumerable<ClubMembership>> GetMembershipsByUserIdAsync(Guid userId);
    Task<IEnumerable<ClubMembership>> GetPendingMembershipsAsync(Guid? clubId = null);
    Task<ClubMembership> RequestMembershipAsync(Guid clubId, Guid userId);
    Task<bool> ApproveMembershipAsync(Guid membershipId, Guid reviewerId, string? note = null);
    Task<bool> RejectMembershipAsync(Guid membershipId, Guid reviewerId, string? note = null);
    Task<bool> CancelMembershipRequestAsync(Guid clubId, Guid userId);
    Task<bool> UpdateMembershipRoleAsync(Guid membershipId, ClubRole newRole);
    Task<bool> RemoveMembershipAsync(Guid membershipId);
    Task<bool> IsUserMemberOfClubAsync(Guid userId, Guid clubId);
    Task<bool> CanUserJoinClubAsync(Guid userId, ClubRole requestedRole);
}

