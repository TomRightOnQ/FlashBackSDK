using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// RegEx for writing to the UIConfig
/// </summary>
public static class UIConfigWriter
{
    private const string ConfigFilePath = "Assets/Scripts/Contents/Configs/UIConfig.cs";

    public static void UpdateOrCreateUIDataEntry(string prefabName, UIData newData)
    {
       string fileContent = File.ReadAllText(ConfigFilePath);

        // Pattern to find the start and end of the data dictionary
        string dictionaryPattern = @"\s*public static Dictionary<string, UIData> data = new Dictionary<string, UIData>\s*\{\s*(.*?)\s*\};";
        Match dictionaryMatch = Regex.Match(fileContent, dictionaryPattern, RegexOptions.Singleline);

        if (dictionaryMatch.Success)
        {
            string dictionaryContent = dictionaryMatch.Groups[1].Value;
            string entryReplacement = $"{{\"{prefabName}\", new UIData(\"{newData.Path}\", UILayer.{newData.Layer})}},";

            // If the dictionary is empty, just add the new entry
            if (string.IsNullOrEmpty(dictionaryContent))
            {
                entryReplacement = "\t\t" + entryReplacement;
                dictionaryContent = entryReplacement;
            }
            else
            {
                // Check if the entry already exists and replace it
                string entryPattern = $@"\{{\s*""{Regex.Escape(prefabName)}""\s*,\s*new\s+UIData\(.+?\)\s*\}},?";
                if (Regex.IsMatch(dictionaryContent, entryPattern))
                {
                    dictionaryContent = Regex.Replace(dictionaryContent, entryPattern, entryReplacement);
                }
                else
                {
                    // If the entry does not exist, add it to the end of the dictionary content
                    if (!dictionaryContent.Trim().EndsWith(","))
                    {
                        dictionaryContent += ",";
                    }
                    dictionaryContent += "\n" + "\t\t" + entryReplacement;
                }
            }

            string updatedDictionaryContent = dictionaryMatch.Value.Replace(dictionaryMatch.Groups[1].Value, dictionaryContent);
            fileContent = fileContent.Substring(0, dictionaryMatch.Index) + updatedDictionaryContent + fileContent.Substring(dictionaryMatch.Index + dictionaryMatch.Length);

            // Write the updated content back to the file
            File.WriteAllText(ConfigFilePath, fileContent);
        }
    }
}