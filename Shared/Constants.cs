static class Constants {
    public const string ASSEMBLY_NAME = "Generated.dll";
    public const string FACIT = 
        @"[Authorize]
[HttpGet(""transfer/{amount:int}/{from}/{to}"")]
public async Task<int> Transfer(int amount, string from, string to)
{
    var fromAccount = await _accountRepository.FindByIdAsync(from);
    var toAccount = await _accountRepository.FindByIdAsync(to);

    if(amount > 0) {
        if (fromAccount.Balance < amount)
        {
            throw new ArgumentException(""Insufficient funds"");
        }

        toAccount.Balance += amount;
        await _accountRepository.SaveAsync(toAccount);

        fromAccount.Balance -= amount;
        await _accountRepository.SaveAsync(fromAccount);
    }
    return fromAccount.Balance;
}";

    public const string INITIAL_IMPLEMENTATION = 
        @"[Authorize]
[HttpGet(""transfer/{amount:int}/{from}/{to}"")]
public async Task<int> Transfer(int amount, string from, string to)
{
    var fromAccount = await _accountRepository.FindByIdAsync(from);
    var toAccount = await _accountRepository.FindByIdAsync(to);

    toAccount.Balance += amount;
    await _accountRepository.SaveAsync(toAccount);

    fromAccount.Balance -= amount;
    await _accountRepository.SaveAsync(fromAccount);

    return fromAccount.Balance;
}";

public const string PROGRAM_CODE = @"public partial class Program { 
public static void Main(string[] args) {
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<IRepository<Account>, AccountRepository>();

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
app.Run();
}
}";

public const string ACCOUNT_MODELS = @"
public struct Account
{
    public string Id { get; }
    public int Balance { get; set; }

    public Account(string id, int? balance)
    {
        Id = id;
        Balance = balance ?? 0;
    }
}

public interface IRepository<T>
{
    Task<T> FindByIdAsync(string id);
    Task SaveAsync(T account);
}

public class AccountRepository : IRepository<Account>
{
    Dictionary<string, Account> _accounts = new Dictionary<string, Account>();

    public AccountRepository()
    {
        _accounts.Add(""1"", new Account(""1"", 1000));
        _accounts.Add(""2"", new Account(""2"", 2000));
    }

    public Task<Account> FindByIdAsync(string accountId)
    {
        if(_accounts.ContainsKey(accountId))
            return Task.FromResult(_accounts[accountId]);
        throw new ArgumentException(""Non existing account"");
    }

    public Task SaveAsync(Account account)
    {
        _accounts[account.Id] = account;
        return Task.CompletedTask;
    }
}";

public const string BANKING_API_CONTROLLER = @"
[ApiController]
[Route(""[controller]"")]
public class BankingController : ControllerBase
{
    private readonly ILogger<BankingController> _logger;
    private readonly IRepository<Account> _accountRepository;

    public BankingController(ILogger<BankingController> logger, IRepository<Account> accountRepository)
    {
        _logger = logger;
        _accountRepository = accountRepository;
    }";

    public const string USINGS =
@"using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
";

    public const string TESTS = 
@"
[TestFixture]
public class UnitTests {

    [Test]
    public void CanTransferBalance()
    {
        // arrange
        var controller = new BankingController(null, new AccountRepository());
        // act
        var result = controller.Transfer(100, ""1"", ""2"").Result;
        // assert
        Assert.True(result == 900);
    }

    [Test]
    public void NegativeInputNotAccepted()
    {
        // arrange
        var controller = new BankingController(null, new AccountRepository());
        // act
        var result = controller.Transfer(-100, ""1"", ""2"").Result;
        // assert
        Assert.False(result == 1100);
    }

    [Test]
    public void NonExistentAccountsShouldReturn0()
    {
        // arrange
        var controller = new BankingController(null, new AccountRepository());
        // act
        var result = controller.Transfer(100, ""3"", ""4"").Result;
        // assert
        Assert.True(result == 0);
    }

    [Test]
    public void NotEnoughFundsShouldThrowException()
    {
        // arrange
        var controller = new BankingController(null, new AccountRepository());
        // act
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => {
            var result = await controller.Transfer(10000, ""1"", ""2"");
        });
        // assert
        Console.WriteLine(exception);
        //Assert.AreEqual(""Insufficient funds"", exception.Message);
    }
}";

    public const string TEST_RUNNER = 
        @"
public class EditorTest
{
    public string? Name { get; set; }
    public bool Success { get; set; }
}

namespace BlazorTesting {
public class TestRunner {
    public EditorTest[] Execute() {
        var count = 0;
        var testResults = new List<EditorTest>();
        var assemblies = new [] { typeof(UnitTests).Assembly };
        var testFixtures = assemblies.SelectMany(a => a.ExportedTypes)
                                     .Where(t => t.IsDefined(typeof(TestFixtureAttribute)));
        var tests = testFixtures.SelectMany(t => t.GetMethods()
                                    .Where(m => m.GetCustomAttributes(typeof(TestAttribute), true).Any()));
        
        foreach(var test in tests) {
            var testResult = new EditorTest();
            testResult.Name = test.Name;
            try {
                var obj = Activator.CreateInstance(test.DeclaringType);
                test.Invoke(obj, null);
                testResult.Success = true;
            } catch(Exception ex) {
                count++;
            }
            testResults.Add(testResult);
        }

        return testResults.ToArray();
    }
}
}
";

}