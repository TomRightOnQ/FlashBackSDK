using System;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public static class UIConfig
{
    public static Dictionary<string, string> data = new Dictionary<string, string>
    {
        {"UI_NewWidget", ""},
		{"UI_Sample", "Assets/Resources/Art/UI/Widgets/Sample/UI_Sample.prefab"},
    };

    public static Dictionary<string, Type> widgetName = new Dictionary<string, Type>
    {
        {"Btn_", typeof(UnityEngine.UI.Button)}, // Button
        {"P_", typeof(UnityEngine.GameObject)}, // Panel
        {"SL_", typeof(UnityEngine.UI.Slider)}, // Panel
        {"TB_", typeof(TMPro.TextMeshProUGUI)}, // TextBlock
        {"IF_", typeof(TMPro.TMP_InputField)}, // InputField
        {"Img_", typeof(UnityEngine.UI.Image)}, // Image
        {"TG_", typeof(UnityEngine.UI.Toggle)}, // Toggle
    };

    public static Dictionary<Type, string> callBack = new Dictionary<Type, string>
    {
        {typeof(UnityEngine.UI.Button), "OnClick_"}, // Button
        {typeof(UnityEngine.UI.Slider), "OnValueChange_"}, // Panel
        {typeof(UnityEngine.UI.Toggle), "OnValueChange_"}, // Toggle
    };
}

