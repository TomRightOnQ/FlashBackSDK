using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System;

/// <summary>
/// 对话轨道内的单个片段
/// </summary>
[Serializable]
public class FBDialogueClip : PlayableAsset, ITimelineClipAsset
{
    [Header("Dialogue Content")]
    public DialogueSingleData DialogueData = new DialogueSingleData();

    [Header("Clip Settings")]
    [Tooltip("Automatically calculate duration based on text length")]
    public bool AutoCalculateDuration = true;

    [Tooltip("Characters per second for auto duration calculation")]
    public float CharactersPerSecond = 20f;

    [Tooltip("Minimum clip duration in seconds")]
    public float MinDuration = 2f;

    [Tooltip("Additional time for choices")]
    public float ChoiceBufferTime = 1f;

    // ITimelineClipAsset implementation
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending | ClipCaps.Extrapolation; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<FBDialogueBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        // Pass data to behaviour
        behaviour.DialogueData = DialogueData;
        behaviour.IsLastClip = false; // This will be set by the track

        return playable;
    }

    /// <summary>
    /// Calculate automatic duration based on text length
    /// </summary>
    public double CalculateAutoDuration()
    {
        if (string.IsNullOrEmpty(DialogueData.DialogueContent))
            return MinDuration;

        float baseTime = DialogueData.DialogueContent.Length / CharactersPerSecond;
        float choiceTime = DialogueData.ChoiceCount > 0 ? ChoiceBufferTime : 0f;

        return Mathf.Max(baseTime + choiceTime, MinDuration);
    }
}

[Serializable]
public class FBDialogueBehaviour : PlayableBehaviour
{
    public DialogueSingleData DialogueData;
    public bool IsLastClip;

    private bool isPlaying = false;
    private int selectedChoiceIndex = -1;

    public override void OnGraphStart(Playable playable)
    {
        isPlaying = false;
        selectedChoiceIndex = -1;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (!isPlaying)
        {
            // Show dialogue using the fictional API
            // DialogueSystem.ShowDialogue(DialogueData);

            // If this clip has choices and should pause, set up pause logic
            if (DialogueData.ChoiceCount > 0 && DialogueData.bPauseAtEnd)
            {
                // Store reference for choice selection
                FBDialogueTrackManager.RegisterActiveClip(this);
            }

            isPlaying = true;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (isPlaying)
        {
            // If this is the last clip and has choices, trigger the finish event
            if (IsLastClip && DialogueData.ChoiceCount > 0)
            {
                int finalChoiceIndex = selectedChoiceIndex >= 0 ? selectedChoiceIndex : 0;
                OnDialogueSectionFinished(finalChoiceIndex);
            }
            else
            {
                // Not the last clip or no choices, pass 0
                OnDialogueSectionFinished(0);
            }

            isPlaying = false;
        }
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // Check for choice selection if this clip is paused at end
        if (isPlaying && DialogueData.bPauseAtEnd && DialogueData.ChoiceCount > 0)
        {
            // This would be called every frame while the clip is active
            // You could check for input or other conditions here
        }
    }

    /// <summary>
    /// Called when a choice is selected (from UI or input)
    /// </summary>
    public void OnChoiceSelected(int choiceIndex)
    {
        if (choiceIndex >= 0 && choiceIndex < DialogueData.ChoiceCount)
        {
            selectedChoiceIndex = choiceIndex;

            // If not the last clip, immediately continue
            if (!IsLastClip)
            {
                OnDialogueSectionFinished(0);
            }
        }
    }

    /// <summary>
    /// Fictional API call - this would be implemented in your game system
    /// </summary>
    private void OnDialogueSectionFinished(int choiceIndex)
    {
        // This is where you'd call your actual game system
        Debug.Log($"Dialogue section finished with choice index: {choiceIndex}");

        // Fictional API call
        // DialogueSystem.OnDialogueSectionFinished(choiceIndex);
    }
}