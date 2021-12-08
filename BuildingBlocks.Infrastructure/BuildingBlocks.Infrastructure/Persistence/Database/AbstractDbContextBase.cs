using System.Data;
using Enmeshed.BuildingBlocks.Application.Abstractions.Infrastructure.Persistence.Database;
using Enmeshed.DevelopmentKit.Identity.ValueObjects;
using Enmeshed.BuildingBlocks.Infrastructure.Persistence.Database.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Enmeshed.BuildingBlocks.Infrastructure.Persistence.Database
{
    public class AbstractDbContextBase : DbContext, IDbContext
    {
        private const int MAX_RETRY_COUNT = 50000;
        private static readonly TimeSpan MAX_RETRY_DELAY = TimeSpan.FromSeconds(1);

        protected AbstractDbContextBase()
        {
        }

        protected AbstractDbContextBase(DbContextOptions options) : base(options)
        {
        }

        public IQueryable<T> SetReadOnly<T>() where T : class
        {
            return Set<T>().AsNoTracking();
        }

        public async Task RunInTransaction(Func<Task> action, List<int>? errorNumbersToRetry,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            var executionStrategy =
                new SqlServerRetryingExecutionStrategy(this, MAX_RETRY_COUNT, MAX_RETRY_DELAY, errorNumbersToRetry);

            await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await Database.BeginTransactionAsync(isolationLevel);
                await action();
                await transaction.CommitAsync();
            });
        }

        public async Task RunInTransaction(Func<Task> action, IsolationLevel isolationLevel)
        {
            await RunInTransaction(action, null, isolationLevel);
        }

        public async Task<T?> RunInTransaction<T>(Func<Task<T?>> func, List<int>? errorNumbersToRetry,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            var response = default(T);

            await RunInTransaction(async () => { response = await func(); }, errorNumbersToRetry, isolationLevel);

            return response;
        }

        public async Task<T?> RunInTransaction<T>(Func<Task<T?>> func, IsolationLevel isolationLevel)
        {
            return await RunInTransaction(func, null, isolationLevel);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.UseValueConverter(
                new UsernameValueConverter(
                    new ConverterMappingHints(Username.MAX_LENGTH)));

            modelBuilder.UseValueConverter(
                new IdentityAddressValueConverter(new ConverterMappingHints(IdentityAddress.MAX_LENGTH)));

            modelBuilder.UseValueConverter(new DeviceIdValueConverter(new ConverterMappingHints(DeviceId.MAX_LENGTH)));

            modelBuilder.UseValueConverter(new DateTimeValueConverter());
            modelBuilder.UseValueConverter(new NullableDateTimeValueConverter());
        }

        protected void RollBack()
        {
            var changedEntries = ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged).ToList();

            foreach (var entry in changedEntries)
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
        }
    }
}