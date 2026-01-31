using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("UI References")]
    public Slider progressSlider;
    public TextMeshProUGUI timerText;

    [Header("Spawn Settings")]
    [Range(0.1f, 1.0f)] public float spawnAreaScale = 0.5f;
    public float screenPadding = 0.5f;

    [Header("Game Difficulty")]
    public bool useRandomKeys = false; // Tích vào đây nếu là Level cuối
    public float missPenalty = 5f;     // Trừ 5% nếu bấm trượt

    public bool IsGameFinished { get; private set; } = false;

    private LevelData _currentLevelData;
    private float _levelTimer = 0f;
    private float _currentProgress = 0f;
    private float _nextSpawnTimer = 0f;
    private Vector2 _lastNotePosition = Vector2.zero;
    
    private int _currentKeyIndex = 0;
    // Danh sách phím
    private readonly KeyCode[] _sequenceKeys = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Space };

    void Awake() => Instance = this;

    void Start() { }

    void Update()
    {
        if (_currentLevelData == null || IsGameFinished) return;

        // 1. Timer Logic
        _levelTimer -= Time.deltaTime;
        if (timerText != null) timerText.text = "Timer: "+Mathf.CeilToInt(_levelTimer).ToString();

        if (_levelTimer <= 0)
        {
            Debug.Log("Time Over");
            FinishGame(); // Hết giờ thì kết thúc
            return;
        }

        // 2. Spawn Logic
        _nextSpawnTimer -= Time.deltaTime;
        if (_nextSpawnTimer <= 0)
        {
            SpawnNote();
            _nextSpawnTimer = Random.Range(_currentLevelData.minSpawnDelay, _currentLevelData.maxSpawnDelay);
        }
    }

    public void StartLevel(LevelData data)
    {
        if (data == null)
        {
            IsGameFinished = true; 
            return;
        }

        _currentLevelData = data;
        AudioManager.Instance.PlayMusic((_currentLevelData.levelName));
        IsGameFinished = false;
        _levelTimer = _currentLevelData.levelDuration;
        _currentProgress = 0f;
        _lastNotePosition = Vector2.zero;
        
        if (progressSlider != null) progressSlider.value = 0f;
        ClearAllNotes();
    }

    public void AddProgress()
    {
        if (IsGameFinished) return;

        _currentProgress += _currentLevelData.progressPerHit;
        // Kẹp giá trị không quá 100
        _currentProgress = Mathf.Clamp(_currentProgress, 0f, 100f);
        
        if (progressSlider != null) progressSlider.value = _currentProgress / 100f;

        if (_currentProgress >= 100f)
        {
            Debug.Log("Level Complete!");
            FinishGame();
        }
    }

    // --- HÀM MỚI: TRỪ ĐIỂM KHI HỤT ---
    public void SubtractProgress()
    {
        if (IsGameFinished) return;

        _currentProgress -= missPenalty; 
        // Kẹp giá trị không dưới 0
        _currentProgress = Mathf.Clamp(_currentProgress, 0f, 100f);

        if (progressSlider != null) progressSlider.value = _currentProgress / 100f;
        
        Debug.Log("Miss! Process decreased.");
    }
    // ---------------------------------

    private void FinishGame()
    {
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayMusic("default");
        IsGameFinished = true; 
        ClearAllNotes();       
    }

    private void ClearAllNotes()
    {
        HitObject[] activeNotes = FindObjectsOfType<HitObject>();
        foreach (var note in activeNotes) Destroy(note.gameObject);
    }

    private void SpawnNote()
    {
        Vector2 rawPos = GetRelativeRandomPosition();
        Vector2 finalPos = ClampToCameraView(rawPos);

        // Instantiate Note
        GameObject go = Instantiate(_currentLevelData.notePrefab, finalPos, Quaternion.identity);
        HitObject hitObj = go.GetComponent<HitObject>();

        // --- LOGIC RANDOM KEY ---
        KeyCode targetKey;
        if (useRandomKeys)
        {
            // Level cuối: Chọn ngẫu nhiên trong danh sách phím
            int randIndex = Random.Range(0, _sequenceKeys.Length);
            targetKey = _sequenceKeys[randIndex];
        }
        else
        {
            // Level thường: Theo thứ tự tuần tự
            targetKey = _sequenceKeys[_currentKeyIndex];
            _currentKeyIndex = (_currentKeyIndex + 1) % _sequenceKeys.Length;
        }
        // ------------------------

        HitObjectData data = new HitObjectData {
            hitKey = targetKey,
            hitTime = Random.Range(_currentLevelData.minApproachTime, _currentLevelData.maxApproachTime),
            position = finalPos
        };

        hitObj.hitWindow = _currentLevelData.hitWindow;
        hitObj.Initialize(data);

        _lastNotePosition = finalPos;
    }
    
    // ... (Giữ nguyên GetRelativeRandomPosition và ClampToCameraView) ...
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
        float fullHeight = cam.orthographicSize;
        float fullWidth = fullHeight * cam.aspect;
        float spawnHeight = fullHeight * spawnAreaScale;
        float spawnWidth = fullWidth * spawnAreaScale;

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