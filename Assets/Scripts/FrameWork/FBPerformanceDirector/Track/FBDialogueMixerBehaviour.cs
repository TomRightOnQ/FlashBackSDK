using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using UnityEngine.Timeline;

public class FBDialogueMixerBehaviour : PlayableBehaviour
{
    public FBDialogueTrack Track;
    public IEnumerable<TimelineClip> Clips;

    private FBDialogueBehaviour activeBehaviour;
    private int currentClipIndex = -1;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        int inputCount = playable.GetInputCount();
        bool foundActiveClip = false;

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);

            if (inputWeight > 0f)
            {
                ScriptPlayable<FBDialogueBehaviour> inputPlayable = (ScriptPlayable<FBDialogueBehaviour>)playable.GetInput(i);
                FBDialogueBehaviour input = inputPlayable.GetBehaviour();

                if (!foundActiveClip)
                {
                    // This is the active clip
                    if (activeBehaviour != input)
                    {
                        // New clip became active
                        activeBehaviour = input;
                        currentClipIndex = i;
                    }
                    foundActiveClip = true;
                }
            }
        }

        // If no clip is active, clear the active behaviour
        if (!foundActiveClip && activeBehaviour != null)
        {
            activeBehaviour = null;
            currentClipIndex = -1;
        }
    }

    public override void OnGraphStart(Playable playable)
    {
        // Ensure last clips are marked when graph starts
        if (Track != null)
        {
            Track.MarkLastClips();
        }
    }
}