namespace BlazorEditor.Data;

public class EditorService
{
    public Task<EditorTest[]> RunTestsAsync(string code)
    {
        return Task.FromResult(new List<EditorTest>().ToArray());
    }
}
