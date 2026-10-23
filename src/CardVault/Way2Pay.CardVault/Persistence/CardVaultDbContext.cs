using Microsoft.EntityFrameworkCore;
using Way2Pay.CardVault.Cards;

namespace Way2Pay.CardVault.Persistence;

internal sealed class CardVaultDbContext(DbContextOptions<CardVaultDbContext> options) : DbContext(options)
{
    public DbSet<StoredCard> StoredCards => Set<StoredCard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CardVaultDbContext).Assembly);
    }
}
