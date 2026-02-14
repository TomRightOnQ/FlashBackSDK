using Unity;
using System;
using System.Collections.Generic;

/// <summary>
/// Struct that holds the data for single choice
/// </summary>
[System.Serializable]
public struct DialogueSingleData
{
    public string SpeakerName;
    public string DialogueContent;

    public bool bHideChatBox;
    public bool bHideChoice;
    public bool bAllowSkip;
    public bool bPauseAtEnd;

    public int ChoiceCount;
    public List<string> ChoiceContent;

    public DialogueSingleData(string speakerName, string dialogueContent)
    {
        SpeakerName = speakerName;
        DialogueContent = dialogueContent;
        bHideChatBox = false;
        bHideChoice = false;
        bAllowSkip = true;
        bPauseAtEnd = false;
        ChoiceCount = 0;
        ChoiceContent = new List<string>();
    }
}
