using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; 

    [Header("UI References")]
    public Slider progressSlider; 
    public TextMeshProUGUI timerText; 

    [Header("Data Source")]
    public LevelPlaylist playlist;

    [Header("Spawn Settings")]
    [Range(0.1f, 1.0f)] public float spawnAreaScale = 0.5f; // Hệ số vùng spawn (0.5 = 50%)
    public float screenPadding = 0.5f; 

    private LevelData _currentLevelData;
    private int _currentLevelIndex = 0;
    private float _levelTimer = 0f;
    private float _currentProgress = 0f; 
    private float _nextSpawnTimer = 0f;
    private Vector2 _lastNotePosition = Vector2.zero; 
    private int _currentKeyIndex = 0;
    private readonly KeyCode[] _sequenceKeys = { KeyCode.Q, KeyCode.W, KeyCode.E };

    void Awake() => Instance = this;

    void Start()
    {
        if (playlist != null && playlist.levels.Count > 0) LoadLevel(0);
    }

    void Update()
    {
        if (_currentLevelData == null) return;

        _levelTimer -= Time.deltaTime;
        if (timerText != null) timerText.text = Mathf.CeilToInt(_levelTimer).ToString();

        if (_levelTimer <= 0)
        {
            RestartLevel();
            return;
        }

        _nextSpawnTimer -= Time.deltaTime;
        if (_nextSpawnTimer <= 0)
        {
            SpawnNote();
            _nextSpawnTimer = Random.Range(_currentLevelData.minSpawnDelay, _currentLevelData.maxSpawnDelay);
        }
    }

    public void AddProgress()
    {
        _currentProgress += _currentLevelData.progressPerHit;
        if (progressSlider != null) progressSlider.value = _currentProgress / 100f;
        if (_currentProgress >= 100f) LoadNextLevel();
    }

    private void RestartLevel() => LoadLevel(_currentLevelIndex);

    private void LoadNextLevel()
    {
        _currentLevelIndex = (_currentLevelIndex + 1) % playlist.levels.Count;
        LoadLevel(_currentLevelIndex);
    }

    private void LoadLevel(int index)
    {
        // Xóa các nốt cũ trên màn hình khi chuyển level hoặc restart
        HitObject[] activeNotes = FindObjectsOfType<HitObject>();
        foreach (var note in activeNotes) Destroy(note.gameObject);

        _currentLevelData = playlist.levels[index];
        _currentLevelIndex = index;
        _levelTimer = _currentLevelData.levelDuration;
        _currentProgress = 0f;
        _lastNotePosition = Vector2.zero; 
        if (progressSlider != null) progressSlider.value = 0f;
    }

    private void SpawnNote()
    {
        Vector2 rawPos = GetRelativeRandomPosition();
        Vector2 finalPos = ClampToCameraView(rawPos);

        GameObject go = Instantiate(_currentLevelData.notePrefab, finalPos, Quaternion.identity);
        HitObject hitObj = go.GetComponent<HitObject>();

        KeyCode targetKey = _sequenceKeys[_currentKeyIndex];
        _currentKeyIndex = (_currentKeyIndex + 1) % _sequenceKeys.Length;

        HitObjectData data = new HitObjectData {
            hitKey = targetKey,
            hitTime = Random.Range(_currentLevelData.minApproachTime, _currentLevelData.maxApproachTime),
            position = finalPos
        };

        hitObj.hitWindow = _currentLevelData.hitWindow;
        hitObj.Initialize(data);

        _lastNotePosition = finalPos;
    }
    
    private Vector2 GetRelativeRandomPosition()
    {
        if (_lastNotePosition == Vector2.zero) 
            _lastNotePosition = (Vector2)Camera.main.transform.position;

        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        float distance = Random.Range(_currentLevelData.minStepDistance, _currentLevelData.maxStepDistance);

        return _lastNotePosition + (direction * distance);
    }

    // --- SỬA ĐỔI CHÍNH TẠI ĐÂY ---
    private Vector2 ClampToCameraView(Vector2 targetPos)
    {
        Camera cam = Camera.main;
        
        // Lấy kích thước thực tế của Camera
        float fullHeight = cam.orthographicSize;
        float fullWidth = fullHeight * cam.aspect;

        // Tính toán vùng spawn dựa trên tỉ lệ (spawnAreaScale = 0.5f)
        float spawnHeight = fullHeight * spawnAreaScale;
        float spawnWidth = fullWidth * spawnAreaScale;

        // Giới hạn nốt trong vùng trung tâm
        float minX = cam.transform.position.x - spawnWidth + screenPadding;
        float maxX = cam.transform.position.x + spawnWidth - screenPadding;
        float minY = cam.transform.position.y - spawnHeight + screenPadding;
        float maxY = cam.transform.position.y + spawnHeight - screenPadding;

        return new Vector2(
            Mathf.Clamp(targetPos.x, minX, maxX),
            Mathf.Clamp(targetPos.y, minY, maxY)
        );
    }
}