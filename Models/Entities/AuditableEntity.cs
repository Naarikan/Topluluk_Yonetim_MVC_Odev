using System;
using System.ComponentModel.DataAnnotations;

namespace Topluluk_Yonetim.MVC.Models.Entities;

public abstract class AuditableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? CreatedById { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedById { get; set; }

    public bool IsDeleted { get; set; }
}

