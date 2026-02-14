using UnityEngine;
using UnityEngine.Timeline;
using System.Collections.Generic;

/// <summary>
/// 剧情编辑器使用的特殊TimeLine
/// Timeline for FBPerformance System
/// </summary>
[CreateAssetMenu(fileName = "NewPerformanceTimeline", menuName = "Performance System/Performance Timeline", order = 320)]
[TrackColor(0.2f, 0.8f, 0.2f)]
public class FBPerformanceTimeline : TimelineAsset
{
    [Header("Performance Settings")]
    [Tooltip("Unique identifier for this performance timeline")]
    public string TimelineGUID;

    [Tooltip("Description of this performance")]
    public string Description;

    [Header("Default Tracks")]
    public bool CreateDefaultDialogueTrack = true;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(TimelineGUID))
        {
            TimelineGUID = System.Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Get or create the main dialogue track
    /// </summary>
    public FBDialogueTrack GetDialogueTrack(bool createIfMissing = true)
    {
        foreach (var track in GetOutputTracks())
        {
            if (track is FBDialogueTrack dialogueTrack)
            {
                return dialogueTrack;
            }
        }

        if (createIfMissing)
        {
            var newTrack = CreateTrack<FBDialogueTrack>(null, "Dialogue Track");
            return newTrack as FBDialogueTrack;
        }

        return null;
    }

    /// <summary>
    /// Add a dialogue clip to the timeline
    /// </summary>
    public FBDialogueClip AddDialogueClip(double startTime, string speakerName = "", string dialogueText = "")
    {
        var track = GetDialogueTrack(true);
        return track?.AddDialogueClip(startTime, speakerName, dialogueText);
    }

    /// <summary>
    /// Get all dialogue clips in this timeline
    /// </summary>
    public List<FBDialogueClip> GetAllDialogueClips()
    {
        var track = GetDialogueTrack(false);
        return track?.GetDialogueClips() ?? new List<FBDialogueClip>();
    }

    /// <summary>
    /// Ensure last clips are properly marked
    /// </summary>
    public void RefreshDialogueClips()
    {
        var track = GetDialogueTrack(false);
        track?.MarkLastClips();
    }

    public List<T> GetPerformanceTracks<T>() where T : TrackAsset
    {
        var tracks = new List<T>();
        foreach (var track in GetOutputTracks())
        {
            if (track is T performanceTrack)
            {
                tracks.Add(performanceTrack);
            }
        }
        return tracks;
    }
}