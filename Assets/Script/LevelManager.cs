using UnityEngine;
using UnityEngine.UI; // Cần thư viện này để dùng Slider
using System.Collections.Generic;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; 

    [Header("UI References")]
    public Slider progressSlider; 
    public TextMeshProUGUI timerText; 

    [Header("Data")]
    public LevelPlaylist playlist;
    public float screenPadding = 1.0f;

    private LevelData _currentLevelData;
    private int _currentLevelIndex = 0;
    
    private float _levelTimer = 0f;
    private float _currentProgress = 0f; 
    
    private float _nextSpawnTimer = 0f;
    private Vector2 _lastNotePosition = Vector2.zero;
    private int _currentKeyIndex = 0;
    private readonly KeyCode[] _sequenceKeys = { KeyCode.Q, KeyCode.W, KeyCode.E };

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (playlist != null && playlist.levels.Count > 0)
        {
            LoadLevel(0);
        }
    }

    void Update()
    {
        if (_currentLevelData == null) return;

        // 1. Cập nhật Timer
        _levelTimer -= Time.deltaTime;

        // Cập nhật UI Timer (nếu có)
        if (timerText != null) timerText.text = Mathf.CeilToInt(_levelTimer).ToString();

        // 2. Kiểm tra điều kiện THUA (Hết giờ mà chưa đủ 100%)
        if (_levelTimer <= 0)
        {
            Debug.Log("Hết giờ! Chưa đủ điểm -> Chơi lại!");
            RestartLevel();
            return;
        }

        // 3. Logic Spawn nốt (như cũ)
        _nextSpawnTimer -= Time.deltaTime;
        if (_nextSpawnTimer <= 0)
        {
            SpawnNote();
            _nextSpawnTimer = Random.Range(_currentLevelData.minSpawnDelay, _currentLevelData.maxSpawnDelay);
        }
    }

    // --- HÀM NÀY ĐƯỢC GỌI TỪ HITOBJECT KHI BẤM TRÚNG ---
    public void AddProgress()
    {
        // Cộng % tiến độ
        _currentProgress += _currentLevelData.progressPerHit;
        
        // Cập nhật thanh Slider (Slider trong Unity chạy từ 0 đến 1)
        if (progressSlider != null)
        {
            progressSlider.value = _currentProgress / 100f;
        }

        // Kiểm tra điều kiện THẮNG (Đủ 100%)
        if (_currentProgress >= 100f)
        {
            Debug.Log("Level Complete!");
            LoadNextLevel();
        }
    }

    private void RestartLevel()
    {
        // Load lại chính level hiện tại
        LoadLevel(_currentLevelIndex);
    }

    private void LoadNextLevel()
    {
        _currentLevelIndex++;
        if (_currentLevelIndex >= playlist.levels.Count)
        {
            _currentLevelIndex = 0; // Loop game
            Debug.Log("Game Win! Loop lại từ đầu.");
        }
        LoadLevel(_currentLevelIndex);
    }

    private void LoadLevel(int index)
    {
        _currentLevelData = playlist.levels[index];
        _currentLevelIndex = index;

        // Reset các thông số
        _levelTimer = _currentLevelData.levelDuration;
        _currentProgress = 0f;
        _lastNotePosition = Vector2.zero;
        
        // Reset UI
        if (progressSlider != null) progressSlider.value = 0f;

        Debug.Log($"Load Level: {_currentLevelData.levelName} | Mục tiêu: 100% trong {_levelTimer}s");
    }

    // ... (Giữ nguyên các hàm SpawnNote, GetRelativeRandomPosition, ClampToCameraView cũ của bạn ở dưới)
    private void SpawnNote()
    {
        Vector2 rawPos = GetRelativeRandomPosition();
        Vector2 finalPos = ClampToCameraView(rawPos);

        GameObject go = Instantiate(_currentLevelData.notePrefab, finalPos, Quaternion.identity);
        HitObject hitObj = go.GetComponent<HitObject>();

        KeyCode targetKey = _sequenceKeys[_currentKeyIndex];
        _currentKeyIndex = (_currentKeyIndex + 1) % _sequenceKeys.Length;

        float randomApproach = Random.Range(_currentLevelData.minApproachTime, _currentLevelData.maxApproachTime);

        HitObjectData data = new HitObjectData {
            hitKey = targetKey,
            hitTime = randomApproach,
            position = finalPos
        };

        hitObj.hitWindow = _currentLevelData.hitWindow;
        hitObj.Initialize(data);

        _lastNotePosition = finalPos;
    }
    
    private KeyCode GetNextKeyInSequence()
    {
        KeyCode key = _sequenceKeys[_currentKeyIndex];
        _currentKeyIndex = (_currentKeyIndex + 1) % _sequenceKeys.Length;
        return key;
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

    private Vector2 ClampToCameraView(Vector2 targetPos)
    {
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        float minX = cam.transform.position.x - camWidth + screenPadding;
        float maxX = cam.transform.position.x + camWidth - screenPadding;
        float minY = cam.transform.position.y - camHeight + screenPadding;
        float maxY = cam.transform.position.y + camHeight - screenPadding;
        return new Vector2(Mathf.Clamp(targetPos.x, minX, maxX), Mathf.Clamp(targetPos.y, minY, maxY));
    }
}