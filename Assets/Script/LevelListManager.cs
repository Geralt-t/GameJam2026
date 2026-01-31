// using UnityEngine;
//
// public class LevelListManager : MonoBehaviour
// {
//     public static LevelListManager Instance;
//
//     [Header("Data")]
//     public LevelPlaylist playlist;
//     
//     [Header("State")]
//     public int currentLevelIndex = 0;
//     private LevelData _currentLevelData;
//
//     void Awake()
//     {
//         Instance = this;
//     }
//
//     void Start()
//     {
//         LoadLevel(0);
//     }
//
//     public void LoadLevel(int index)
//     {
//         if (index < 0 || index >= playlist.levels.Count) return;
//
//         currentLevelIndex = index;
//         _currentLevelData = playlist.levels[currentLevelIndex];
//
//
//         Debug.Log($"Đã chuyển sang: {_currentLevelData.levelName}");
//     }
//
//     public void NextLevel()
//     {
//         LoadLevel(currentLevelIndex + 1);
//     }
//
//     public LevelData GetCurrentLevelData() => _currentLevelData;
// }