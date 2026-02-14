using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;

/// <summary>
/// 剧情使用的特殊对话Track，每个Timeline有且只能有一个FBDialogueTrack
/// </summary>
[TrackClipType(typeof(FBDialogueClip))]
[TrackBindingType(typeof(GameObject))]
[TrackColor(0.1f, 0.5f, 0.8f)]
public class FBDialogueTrack : PerformanceTrack
{
    [Header("对话轨道设置")]
    [Tooltip("发言角色名称")]
    public string SpeakerName;

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var mixer = ScriptPlayable<FBDialogueMixerBehaviour>.Create(graph, inputCount);
        var behaviour = mixer.GetBehaviour();

        if (behaviour != null)
        {
            behaviour.Track = this;
            behaviour.Clips = GetClips();
        }

        return mixer;
    }

    /// <summary>
    /// 获取轨道中的所有对话片段
    /// </summary>
    public List<FBDialogueClip> GetDialogueClips()
    {
        var clips = new List<FBDialogueClip>();
        foreach (var timelineClip in GetClips())
        {
            if (timelineClip.asset is FBDialogueClip dialogueClip)
            {
                clips.Add(dialogueClip);
            }
        }
        return clips;
    }

    /// <summary>
    /// 在指定时间添加新的对话片段
    /// </summary>
    public FBDialogueClip AddDialogueClip(double startTime, string speakerName = "", string dialogueText = "")
    {
        var clip = CreateClip<FBDialogueClip>();
        clip.start = startTime;

        var dialogueClip = clip.asset as FBDialogueClip;
        if (dialogueClip != null)
        {
            // Set default values
            dialogueClip.DialogueData.SpeakerName = string.IsNullOrEmpty(speakerName) ? SpeakerName : speakerName;
            dialogueClip.DialogueData.DialogueContent = string.IsNullOrEmpty(dialogueText) ? "Enter dialogue here..." : dialogueText;

            // Auto-calculate duration if enabled
            if (dialogueClip.AutoCalculateDuration)
            {
                clip.duration = dialogueClip.CalculateAutoDuration();
            }
            else
            {
                clip.duration = 2.0; // Default duration
            }

            dialogueClip.DialogueData.bPauseAtEnd = dialogueClip.DialogueData.ChoiceCount > 0;
        }

        return dialogueClip;
    }

    /// <summary>
    /// 设置最后一个片段的标记
    /// </summary>
    public void MarkLastClips()
    {
        var clips = GetClips();
        var dialogueClips = GetDialogueClips();

        if (dialogueClips.Count > 0)
        {
            // Find the last clip by end time
            TimelineClip lastClip = null;
            double maxEndTime = 0;

            foreach (var clip in clips)
            {
                if (clip.asset is FBDialogueClip && clip.end > maxEndTime)
                {
                    maxEndTime = clip.end;
                    lastClip = clip;
                }
            }

            // Mark all clips with their last clip status
            foreach (var clip in clips)
            {
                if (clip.asset is FBDialogueClip dialogueClipAsset)
                {
                    var behaviour = GetDialogueBehaviour(clip);
                    if (behaviour != null)
                    {
                        behaviour.IsLastClip = (clip == lastClip);
                    }
                }
            }
        }
    }

    private FBDialogueBehaviour GetDialogueBehaviour(TimelineClip clip)
    {
        if (clip.asset is FBDialogueClip dialogueClip)
        {
            // This is a simplified approach - in practice you might need to access the playable
            return new FBDialogueBehaviour(); // Placeholder
        }
        return null;
    }
}