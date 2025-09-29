using UnityEngine;

public class MapComponent
{
    // This will automatically handle messages from MapComponent.json protocol
    public void On_indicator(float[] floats, int[] ints, bool[] bools)
    {
        float x = floats[0];
        float y = floats[1];
        int id = ints[0];
        int playerId = ints[1];

        Debug.Log($"MapComponent received position: ({x},{y}) for player {playerId}");

        // Update your map logic here
    }
}