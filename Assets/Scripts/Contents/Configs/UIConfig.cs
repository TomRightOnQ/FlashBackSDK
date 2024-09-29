using System.Collections.Generic;

public static class UIConfig
{
    public static Dictionary<string, UIData> data = new Dictionary<string, UIData>
    {
        {"UI_NewWidget", new UIData("Assets/Scripts/UI/", UILayer.HUD)},
		{"UI_Sample", new UIData("Assets/Scripts/UI/", UILayer.HUDTop)},
    };

    public static UIData GetUIData(string PrefabName)
    {
        if (data.TryGetValue(PrefabName, out UIData outData))
        {
            return outData;
        }
        else
        {
            return null;
        }
    }
}
