namespace NICE.EAT.Models;

/// <summary>
/// Abstract base class for all domain entities.
/// PARENT CLASS #1 — used by MenuItem, Restaurant, User, Order.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>Returns a human-readable type label.</summary>
    public virtual string GetEntityLabel() => GetType().Name;

    public override string ToString() => $"{GetEntityLabel()} [Id={Id}]";

    public override bool Equals(object? obj)
    {
        if (obj is BaseEntity other)
            return Id == other.Id && GetType() == other.GetType();
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
