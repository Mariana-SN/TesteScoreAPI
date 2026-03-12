using Microsoft.EntityFrameworkCore;
using Teste.ScoreAPI.Domain.Entities;
using Teste.ScoreAPI.Infrastructure.Configurations;

namespace Teste.ScoreAPI.Infrastructure.Contexts
{
    public sealed class TestScoreDbContext : DbContext
    {
        public TestScoreDbContext(DbContextOptions<TestScoreDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CustomerEntityTypeConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}