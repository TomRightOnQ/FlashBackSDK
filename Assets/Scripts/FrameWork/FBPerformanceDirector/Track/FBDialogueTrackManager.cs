using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Static manager to handle dialogue choice selection across tracks
/// </summary>
public static class FBDialogueTrackManager
{
    private static FBDialogueBehaviour activeClipWithChoices;
    private static Dictionary<FBDialogueBehaviour, int> pendingChoices = new Dictionary<FBDialogueBehaviour, int>();

    /// <summary>
    /// Register a clip that has active choices waiting for selection
    /// </summary>
    public static void RegisterActiveClip(FBDialogueBehaviour clip)
    {
        activeClipWithChoices = clip;
    }

    /// <summary>
    /// Called when a choice is selected from the UI
    /// </summary>
    public static void OnChoiceSelected(int choiceIndex)
    {
        if (activeClipWithChoices != null)
        {
            activeClipWithChoices.OnChoiceSelected(choiceIndex);
            activeClipWithChoices = null;
        }
        else
        {
            // Store choice for when the clip becomes active
            // This handles cases where choice selection might happen before the clip
            if (!pendingChoices.ContainsKey(activeClipWithChoices))
            {
                pendingChoices[activeClipWithChoices] = choiceIndex;
            }
        }
    }

    /// <summary>
    /// Get any pending choice for a clip
    /// </summary>
    public static int GetPendingChoice(FBDialogueBehaviour clip)
    {
        if (pendingChoices.TryGetValue(clip, out int choiceIndex))
        {
            pendingChoices.Remove(clip);
            return choiceIndex;
        }
        return -1;
    }

    /// <summary>
    /// Clear all pending choices (when timeline stops or resets)
    /// </summary>
    public static void ClearPendingChoices()
    {
        pendingChoices.Clear();
        activeClipWithChoices = null;
    }
}