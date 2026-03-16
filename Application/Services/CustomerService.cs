using Teste.ScoreAPI.Application.Contracts;
using Teste.ScoreAPI.Application.Exceptions;
using Teste.ScoreAPI.Application.Interfaces;
using Teste.ScoreAPI.Domain.Entities;
using Teste.ScoreAPI.Domain.Interfaces;

namespace Teste.ScoreAPI.Application.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICpfValidator _cpfValidator;
    private readonly IScoreCalculator _scoreCalculator;

    public CustomerService(
        ICustomerRepository customerRepository,
        ICpfValidator cpfValidator,
        IScoreCalculator scoreCalculator)
    {
        _customerRepository = customerRepository;
        _cpfValidator = cpfValidator;
        _scoreCalculator = scoreCalculator;
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequiredFields(request);

        var normalizedCpf = NormalizeAndValidateCpf(request.Cpf!);

        if (await _customerRepository.ExistsByCpfAsync(normalizedCpf, cancellationToken))
        {
            throw new ConflictException("CPF já cadastrado.");
        }

        if (request.BirthDate!.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ValidationException("Data de nascimento não pode ser futura.");
        }

        if (request.AnnualIncome!.Value < 0)
        {
            throw new ValidationException("Renda anual não pode ser negativa.");
        }

        var customer = new Customer(
            Guid.NewGuid(),
            request.Name?.Trim(),
            request.Email?.Trim(),
            request.BirthDate.Value,
            new Phone(request.Phone!.Ddd!.Trim(), request.Phone.Number!.Trim()),
            normalizedCpf,
            new Address(
                request.Address!.Street?.Trim(),
                request.Address.Number?.Trim(),
                request.Address.Complement?.Trim(),
                request.Address.ZipCode?.Trim(),
                request.Address.State!.Trim().ToUpperInvariant()),
            request.AnnualIncome.Value);

        await _customerRepository.AddAsync(customer, cancellationToken);

        var phoneAlreadyExists = await _customerRepository.ExistsByPhoneExcludingAsync(
            customer.Phone.Ddd,
            customer.Phone.Number,
            customer.Cpf,
            cancellationToken);

        return ToResponse(customer, phoneAlreadyExists);
    }

    public async Task<CustomerResponse?> UpdateByCpfAsync(string cpf, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            throw new ValidationException("CPF é obrigatório.");
        }

        ValidateRequiredFields(request);

        var normalizedCpf = NormalizeAndValidateCpf(cpf);
        var existingCustomer = await _customerRepository.GetByCpfAsync(normalizedCpf, cancellationToken);
        if (existingCustomer is null)
        {
            return null;
        }

        if (request.BirthDate!.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ValidationException("Data de nascimento não pode ser futura.");
        }

        if (request.AnnualIncome!.Value < 0)
        {
            throw new ValidationException("Renda anual não pode ser negativa.");
        }

        var updatedCustomer = new Customer(
            existingCustomer.Id,
            request.Name?.Trim(),
            request.Email?.Trim(),
            request.BirthDate.Value,
            new Phone(request.Phone!.Ddd!.Trim(), request.Phone.Number!.Trim()),
            existingCustomer.Cpf,
            new Address(
                request.Address!.Street?.Trim(),
                request.Address.Number?.Trim(),
                request.Address.Complement?.Trim(),
                request.Address.ZipCode?.Trim(),
                request.Address.State!.Trim().ToUpperInvariant()),
            request.AnnualIncome.Value);

        await _customerRepository.UpdateByCpfAsync(normalizedCpf, updatedCustomer, cancellationToken);

        var phoneAlreadyExists = await _customerRepository.ExistsByPhoneExcludingAsync(
            updatedCustomer.Phone.Ddd,
            updatedCustomer.Phone.Number,
            updatedCustomer.Cpf,
            cancellationToken);

        return ToResponse(updatedCustomer, phoneAlreadyExists);
    }

    public async Task<CustomerResponse?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            throw new ValidationException("CPF é obrigatório.");
        }

        var normalizedCpf = NormalizeAndValidateCpf(cpf);

        var customer = await _customerRepository.GetByCpfAsync(normalizedCpf, cancellationToken);

        if (customer is null) return null;

        var phoneAlreadyExists = await _customerRepository.ExistsByPhoneExcludingAsync(
            customer.Phone.Ddd,
            customer.Phone.Number,
            customer.Cpf,
            cancellationToken);

        return ToResponse(customer, phoneAlreadyExists);
    }

    public async Task<IReadOnlyCollection<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);

        var phoneCounts = customers
            .GroupBy(c => (c.Phone.Ddd, c.Phone.Number))
            .ToDictionary(g => g.Key, g => g.Count());

        return customers
            .Select(c => ToResponse(c, phoneCounts[(c.Phone.Ddd, c.Phone.Number)] > 1))
            .ToArray();
    }

    private CustomerResponse ToResponse(Customer customer, bool phoneAlreadyExists)
    {

        var score = _scoreCalculator.Calculate(
            customer,
            DateOnly.FromDateTime(DateTime.UtcNow),
            phoneAlreadyExists);

        return new CustomerResponse
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            BirthDate = customer.BirthDate,
            Phone = new PhoneResponse
            {
                Ddd = customer.Phone.Ddd,
                Number = customer.Phone.Number
            },
            Cpf = customer.Cpf,
            Address = new AddressResponse
            {
                Street = customer.Address.Street,
                Number = customer.Address.Number,
                Complement = customer.Address.Complement,
                ZipCode = customer.Address.ZipCode,
                State = customer.Address.State
            },
            AnnualIncome = customer.AnnualIncome,
            Score = score
        };
    }

    private static void ValidateRequiredFields(CreateCustomerRequest request)
    {
        if (request.BirthDate is null)
        {
            throw new ValidationException("Data de nascimento é obrigatória.");
        }

        if (request.Phone is null)
        {
            throw new ValidationException("Telefone é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Phone.Ddd))
        {
            throw new ValidationException("DDD do telefone é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Phone.Number))
        {
            throw new ValidationException("Número do telefone é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Cpf))
        {
            throw new ValidationException("CPF é obrigatório.");
        }

        if (request.Address is null)
        {
            throw new ValidationException("Endereço é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Address.State))
        {
            throw new ValidationException("UF do endereço é obrigatória.");
        }

        if (request.Address.State.Trim().Length != 2)
        {
            throw new ValidationException("UF do endereço deve conter 2 caracteres.");
        }

        if (request.AnnualIncome is null)
        {
            throw new ValidationException("Renda anual é obrigatória.");
        }
    }

    private static void ValidateRequiredFields(UpdateCustomerRequest request)
    {
        if (request.BirthDate is null)
        {
            throw new ValidationException("Data de nascimento é obrigatória.");
        }

        if (request.Phone is null)
        {
            throw new ValidationException("Telefone é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Phone.Ddd))
        {
            throw new ValidationException("DDD do telefone é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Phone.Number))
        {
            throw new ValidationException("Número do telefone é obrigatório.");
        }

        if (request.Address is null)
        {
            throw new ValidationException("Endereço é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Address.State))
        {
            throw new ValidationException("UF do endereço é obrigatória.");
        }

        if (request.Address.State.Trim().Length != 2)
        {
            throw new ValidationException("UF do endereço deve conter 2 caracteres.");
        }

        if (request.AnnualIncome is null)
        {
            throw new ValidationException("Renda anual é obrigatória.");
        }
    }

    private string NormalizeAndValidateCpf(string cpf)
    {
        var normalizedCpf = _cpfValidator.Normalize(cpf);
        if (!_cpfValidator.IsValid(normalizedCpf))
        {
            throw new ValidationException("CPF inválido.");
        }

        return normalizedCpf;
    }
}