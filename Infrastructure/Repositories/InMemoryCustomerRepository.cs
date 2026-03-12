using System.Collections.Concurrent;
using Teste.ScoreAPI.Domain.Entities;
using Teste.ScoreAPI.Domain.Interfaces;

namespace Teste.ScoreAPI.Infrastructure.Repositories;

public sealed class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly ConcurrentDictionary<string, Customer> _customersByCpf = new();

    public Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_customersByCpf.ContainsKey(cpf));
    }

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        if (!_customersByCpf.TryAdd(customer.Cpf, customer))
        {
            throw new InvalidOperationException("CPF já cadastrado.");
        }

        return Task.CompletedTask;
    }

    public Task<bool> UpdateByCpfAsync(string cpf, Customer customer, CancellationToken cancellationToken = default)
    {
        if (!_customersByCpf.ContainsKey(cpf))
        {
            return Task.FromResult(false);
        }

        _customersByCpf[cpf] = customer;
        return Task.FromResult(true);
    }

    public Task<Customer?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        _customersByCpf.TryGetValue(cpf, out var customer);
        return Task.FromResult(customer);
    }

    public Task<IReadOnlyCollection<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Customer> customers = _customersByCpf.Values.ToArray();
        return Task.FromResult(customers);
    }
}
