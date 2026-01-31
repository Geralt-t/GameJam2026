using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelPlaylist", menuName = "RhythmGame/Playlist")]
public class LevelPlaylist : ScriptableObject
{
    public List<LevelData> levels;
}