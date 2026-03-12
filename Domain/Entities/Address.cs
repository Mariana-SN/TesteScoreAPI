namespace Teste.ScoreAPI.Domain.Entities;

public sealed class Address
{
    private Address() {}

    public string? Street { get; }
    public string? Number { get; }
    public string? Complement { get;}
    public string? ZipCode { get; }
    public string State { get; } = string.Empty;

    public Address(string? street, string? number, string? complement, string? zipCode, string state)
    {
        Street = street;
        Number = number;
        Complement = complement;
        ZipCode = zipCode;
        State = state;
    }
}
