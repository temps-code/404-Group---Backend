using System;

namespace Domain.Interfaces
{
    public interface IBaseEntity
    {
        Guid Id { get; set; }
        bool IsActive { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
    }
}
