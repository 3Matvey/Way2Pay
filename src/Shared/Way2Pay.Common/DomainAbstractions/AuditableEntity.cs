namespace Way2Pay.Common.DomainAbstractions;

public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset? UpdatedAt { get; protected set; }

}
