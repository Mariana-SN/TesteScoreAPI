using Microsoft.EntityFrameworkCore;
using Teste.ScoreAPI.Application.Exceptions;
using Teste.ScoreAPI.Domain.Entities;
using Teste.ScoreAPI.Domain.Interfaces;
using Teste.ScoreAPI.Infrastructure.Contexts;

namespace Teste.ScoreAPI.Infrastructure.Repositories
{
    public sealed class CustomerRepository : ICustomerRepository
    {
        private readonly TestScoreDbContext _context;

        public CustomerRepository(TestScoreDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .AsNoTracking()
                .AnyAsync(c => c.Cpf == cpf, cancellationToken);
        }

        public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            await _context.Customers.AddAsync(customer, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Customer?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Phone)
                .Include(c => c.Address)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Cpf == cpf, cancellationToken);
        }

        public async Task<IReadOnlyCollection<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Phone)
                .Include(c => c.Address)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdateByCpfAsync(string cpf, Customer customer, CancellationToken cancellationToken = default)
        {
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Cpf == cpf, cancellationToken);

            if (existingCustomer is null)
                return false;

            _context.Entry(existingCustomer).CurrentValues.SetValues(customer);
            _context.Entry(existingCustomer).Property(c => c.Id).IsModified = false;
            _context.Entry(existingCustomer).Property(c => c.Cpf).IsModified = false;

            _context.Entry(existingCustomer.Phone).CurrentValues.SetValues(customer.Phone);
            _context.Entry(existingCustomer.Address).CurrentValues.SetValues(customer.Address);

            await _context.SaveChangesAsync(cancellationToken);
            return true;

        }

        public async Task<bool> ExistsByPhoneExcludingAsync(string ddd, string number, string cpf, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .AsNoTracking()
                .AnyAsync(c => c.Phone.Ddd == ddd && c.Phone.Number == number && c.Cpf != cpf, cancellationToken);
        }
    }
}