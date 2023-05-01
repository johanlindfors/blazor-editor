static class Constants {
    public const string ASSEMBLY_NAME = "Generated.dll";
    public const string INITIAL_IMPLEMENTATION = 
        @"public class Calculator {
    public int Add(int x, int y) {
        return x - y;
    }
}";

    public const string USINGS =
        @"using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Reflection;
using System.Linq;
using System.Text;

namespace MyMath;
public class EditorTest
{
    public string? Name { get; set; }
    public bool Success { get; set; }
}";

    public const string TESTS = 
        @"
[TestFixture]
public class UnitTests {
    [Test]
    public void TestAdd() {
        // Arrange
        var calculator = new MyMath.Calculator();
        // Act
        var result = calculator.Add(1,3);
        // Arrange
        Assert.IsTrue(result == 4);
    }

    [Test]
    public void TestAddNegativeValue() {
        // Arrange
        var calculator = new MyMath.Calculator();
        // Act
        var result = calculator.Add(1,-3);
        // Arrange
        Assert.IsTrue(result == -1);
    }

    [Test]
    public void TestAddNegativeValue2() {
        // Arrange
        var calculator = new MyMath.Calculator();
        // Act
        var result = calculator.Add(-1,3);
        // Arrange
        Assert.IsTrue(result == 2);
    }
}";

    public const string TEST_RUNNER = 
        @"
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
";

}