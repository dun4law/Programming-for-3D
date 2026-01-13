using UnityEngine;

public class RadarTrackable : MonoBehaviour
{
    public enum TrackType
    {
        Enemy,
        Friendly,
        Neutral,
    }

    [SerializeField]
    private TrackType type = TrackType.Enemy;

    public TrackType Type => type;
}
