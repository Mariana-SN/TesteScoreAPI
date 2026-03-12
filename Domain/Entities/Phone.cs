namespace Teste.ScoreAPI.Domain.Entities;

public sealed class Phone
{
    private Phone(){}

    public string Ddd { get; } = string.Empty;
    public string Number { get; } = string.Empty;

    public Phone(string ddd, string number)
    {
        Ddd = ddd;
        Number = number;
    }
}