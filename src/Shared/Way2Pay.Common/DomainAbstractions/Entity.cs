namespace Way2Pay.Common.DomainAbstractions;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
}
