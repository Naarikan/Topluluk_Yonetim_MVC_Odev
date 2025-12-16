using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Models.Entities;

public class Club : AuditableEntity
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(250)]
    public string? AdvisorName { get; set; }

    public Guid PresidentId { get; set; }

    public ApplicationUser President { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public ICollection<ClubMembership> Memberships { get; set; } = new List<ClubMembership>();

    public ICollection<Event> Events { get; set; } = new List<Event>();

    public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
}

