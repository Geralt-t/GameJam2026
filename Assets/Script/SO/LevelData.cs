using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "RhythmGame/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName;
    public GameObject notePrefab;

    [Header("Game Rules")]
    public float levelDuration = 10.0f; // Thời gian giới hạn
    [Range(1, 100)]
    public float progressPerHit = 10.0f; // Bấm trúng 1 nốt tăng bao nhiêu %

    [Header("Spawn Settings")]
    public float minSpawnDelay = 0.5f;
    public float maxSpawnDelay = 1.2f;

    [Header("Note Settings")]
    public float minApproachTime = 0.5f;
    public float maxApproachTime = 2.0f;
    public float hitWindow = 0.15f;
    
    [Header("Position Settings")]
    public float minStepDistance = 2.0f; 
    public float maxStepDistance = 5.0f;
}