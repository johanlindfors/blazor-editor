using DotNetIsolator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace BlazorEditor.Data;

public class EditorService {
    private IsolatedRuntimeHost _runtimeHost;
    private string _binaryDirectory;
    private string _outputAssemblyPath;
    private string _wasmBinaryDirectory;

    public EditorService() {
        _binaryDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
        _outputAssemblyPath = Path.Combine(_binaryDirectory, Constants.ASSEMBLY_NAME);
        _wasmBinaryDirectory = Path.Combine(_binaryDirectory, "IsolatedRuntimeHost", "WasmAssemblies");
;
        _runtimeHost = new IsolatedRuntimeHost()
            .WithAssemblyLoader(assemblyName =>{
                switch(assemblyName) {
                    case (Constants.ASSEMBLY_NAME):
                        return File.ReadAllBytes(_outputAssemblyPath);
                    case "nunit.framework":
                        return File.ReadAllBytes(Path.Combine(_binaryDirectory, "nunit.framework.dll"));
                }
                return null;
            });
    }

    public Task<MyMath.EditorTest[]> RunTestsAsync(string implementation) {
        var code = new System.Text.StringBuilder();
        code.Append(Constants.USINGS);
        code.Append(implementation);
        code.Append(Constants.TESTS);
        code.Append(Constants.TEST_RUNNER);

        var compilationResult = RoslynCompiler(code.ToString());

        var diagnostics = compilationResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
        if(diagnostics.Any()) {
            foreach(var codeIssue in diagnostics) {
                var issue = new System.Text.StringBuilder();
                issue.AppendLine($"ID: {codeIssue.Id}");
                issue.AppendLine($"Message: {codeIssue.GetMessage()}");
                issue.AppendLine($"Location: {codeIssue.Location.GetLineSpan()}");
                issue.AppendLine($"Severity: {codeIssue.Severity}");
                System.Console.WriteLine(issue.ToString());
            }
            return Task.FromResult<MyMath.EditorTest[]>(
                new[] { new MyMath.EditorTest {
                    Name = "Compilation Failed",
                    Success = false
                }}
            );
        }

        using var runtime = new IsolatedRuntime(_runtimeHost);
        var instance = runtime.CreateObject(Constants.ASSEMBLY_NAME, "MyMath", "TestRunner");
        var result = instance.Invoke<MyMath.EditorTest[]>("Execute");
        return Task.FromResult(result);
    }

    private EmitResult RoslynCompiler(string code) {
        var tree = SyntaxFactory.ParseSyntaxTree(code);
        var references = new[] {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(_binaryDirectory, "nunit.framework.dll")),
            MetadataReference.CreateFromFile(Path.Combine(_wasmBinaryDirectory,"netstandard.dll")),
            MetadataReference.CreateFromFile(Path.Combine(_wasmBinaryDirectory, "System.Linq.dll")),
            MetadataReference.CreateFromFile(Path.Combine(_wasmBinaryDirectory, "System.Console.dll")),
            MetadataReference.CreateFromFile(Path.Combine(_wasmBinaryDirectory, "System.Collections.dll")),
            MetadataReference.CreateFromFile(Path.Combine(_wasmBinaryDirectory, "System.Runtime.dll")),
        };
        var compilation = CSharpCompilation.Create(Constants.INITIAL_IMPLEMENTATION)
            .WithOptions(
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(references)
            .AddSyntaxTrees(tree);
        return compilation.Emit(_outputAssemblyPath);
    }
}
