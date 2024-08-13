using System.Collections.Generic;

public static class UIConfig
{
    public static Dictionary<string, string> data = new Dictionary<string, string>
    {
        {"P_SampleUI", "Sample/P_SampleUI"},
    };

    public static string GetPath(string PrefabName)
    {
        if (data.TryGetValue(PrefabName, out string outPath))
        {
            return outPath;
        }
        else
        {
            return "None";
        }
    }
}