using System.Collections.Generic;

public static class PrefabConfig
{
    public static Dictionary<string, string> data = new Dictionary<string, string>
    {
        {"SamplePrefab", "Prefabs/SamplePrefabs/SamplePrefab"},
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