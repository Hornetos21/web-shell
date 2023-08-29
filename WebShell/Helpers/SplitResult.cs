namespace WebShell.Helpers;

public class SplitResult
{
    public string output { get; set; }
    public string GetOutput(string result)
    {
        var splitResult = result.Split('\n');
        for (var i = 2; i < splitResult.Length; i++)
        {
            if(splitResult[i].Contains(@"\WebShell>exit")) break;
            output += $"{splitResult[i]}\n";
        }

        return output;
    }
}