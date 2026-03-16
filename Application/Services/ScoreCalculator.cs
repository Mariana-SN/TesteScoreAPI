using Teste.ScoreAPI.Application.Interfaces;
using Teste.ScoreAPI.Domain.Entities;
using System.Text.RegularExpressions;

namespace Teste.ScoreAPI.Application.Services;

public sealed class ScoreCalculator : IScoreCalculator
{
    public int Calculate(Customer customer, DateOnly referenceDate, bool phoneAlreadyExists)
    {
        var annualIncomeScore = CalculateAnnualIncomeScore(customer.AnnualIncome);
        var age = CalculateAge(customer.BirthDate, referenceDate);
        var ageScore = CalculateAgeScore(age);
        var stateScore = VerifyScoreByState(customer.Cpf, customer.Address.State);
        var emailScore = VerifyNumberSequenceInEmail(customer.Email!);
        var phoneScore = VerifyDuplicatedPhoneNumber(phoneAlreadyExists);

        return annualIncomeScore + ageScore + stateScore - emailScore - phoneScore;
    }

    private static int CalculateAnnualIncomeScore(decimal annualIncome)
    {
        return annualIncome switch
        {
            > 120000m => 300,
            >= 60000m and <= 120000m => 200,
            _ => 100
        };
    }

    private static int CalculateAgeScore(int age)
    {
        return age switch
        {
            > 40 => 200,
            >= 25 and <= 40 => 150,
            _ => 50
        };
    }

    private static int CalculateAge(DateOnly birthDate, DateOnly referenceDate)
    {
        var age = referenceDate.Year - birthDate.Year;
        if (referenceDate.DayOfYear < birthDate.DayOfYear)
        {
            age--;
        }

        return age;
    }

    private static int VerifyScoreByState(string cpf, string state)
    {
        cpf = new string(cpf.Where(char.IsDigit).ToArray());

        int digitNine = int.Parse(cpf[8].ToString());

        var stateRegionMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "RS", 0 }, 
            { "DF", 1 }, { "GO", 1 }, { "MT", 1 }, { "MS", 1 }, { "TO", 1 },
            { "AC", 2 }, { "AM", 2 }, { "AP", 2 }, { "PA", 2 }, { "RO", 2 }, { "RR", 2 },
            { "CE", 3 }, { "MA", 3 }, { "PI", 3 }, 
            { "AL", 4 }, { "PB", 4 }, { "PE", 4 }, { "RN", 4 }, 
            { "BA", 5 }, { "SE", 5 }, 
            { "MG", 6 }, 
            { "ES", 7 }, { "RJ", 7 },
            { "SP", 8 }, 
            { "PR", 9 }, { "SC", 9 }
        };

        string uf = state.ToUpper();

        if (stateRegionMap.TryGetValue(uf, out int expectedDigit))
        {
            return expectedDigit == digitNine ? 200 : 0;
        }

        return 0;
    }

    public int VerifyNumberSequenceInEmail(string email)
    {
        bool hasDigit = Regex.IsMatch(email, @"\d{5,}");

        return hasDigit ? 70 : 0;
    }

    public int VerifyDuplicatedPhoneNumber(bool phoneAlreadyExists)
    {
        return phoneAlreadyExists ? 50 : 0;
    }
}
