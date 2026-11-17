using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Way2Pay.Payments.Application.Common;
using Way2Pay.Payments.Application.Payments;
using Way2Pay.Payments.Application.Payments.CreatePayment;
using Way2Pay.Payments.Domain.Payments;
using Way2Pay.Payments.Domain.Payments.Attempts;
using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Payments.Infrastructure.Persistence.Configurations;

namespace Way2Pay.Payments.Infrastructure.Persistence
{
    internal class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) 
        : DbContext(options), IUnitOfWork
    {
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PaymentOperation> PaymentOperations => Set<PaymentOperation>();
        public DbSet<PaymentOperationRoute> PaymentOperationRoutes => Set<PaymentOperationRoute>();
        public DbSet<PaymentOperationRouteStep> PaymentOperationRouteSteps => Set<PaymentOperationRouteStep>();
        public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly);
        }

        async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                throw new PaymentConcurrencyException(exception);
            }
            catch (DbUpdateException exception) when (IsPaymentCreationConflict(exception))
            {
                throw new PaymentCreationConflictException();
            }
        }

        private static bool IsPaymentCreationConflict(DbUpdateException exception) =>
            exception.InnerException is SqlException sqlException
            && sqlException.Errors.Cast<SqlError>().Any(error =>
                error.Number == 2627 && error.Message.Contains(
                    $"'{PaymentCreationConfiguration.PrimaryKeyName}'", StringComparison.Ordinal));

        async Task IUnitOfWork.DiscardChangesAsync(CancellationToken cancellationToken)
        {
            var transaction = Database.CurrentTransaction;
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
                await transaction.DisposeAsync();
            }
            ChangeTracker.Clear();
        }

    }
}
