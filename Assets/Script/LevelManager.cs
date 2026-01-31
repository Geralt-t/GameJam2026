using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("UI References")]
    public Slider progressSlider;
    public TextMeshProUGUI timerText;
    
    // Không cần Playlist bắt buộc nữa, vì Data sẽ được truyền vào
    // public LevelPlaylist playlist; 

    [Header("Spawn Settings")]
    [Range(0.1f, 1.0f)] public float spawnAreaScale = 0.5f;
    public float screenPadding = 0.5f;

    // --- BIẾN QUAN TRỌNG CHO GAMEFLOW MANAGER ---
    public bool IsGameFinished { get; private set; } = false;
    // --------------------------------------------

    private LevelData _currentLevelData;
    private float _levelTimer = 0f;
    private float _currentProgress = 0f;
    private float _nextSpawnTimer = 0f;
    private Vector2 _lastNotePosition = Vector2.zero;
    private int _currentKeyIndex = 0;
    private readonly KeyCode[] _sequenceKeys = { KeyCode.Q, KeyCode.W, KeyCode.E };

    void Awake() => Instance = this;

    void Start()
    {
        // QUAN TRỌNG: Xóa logic tự động chạy LoadLevel(0) ở đây.
        // Game sẽ nằm im chờ GameFlowManager gọi lệnh bắt đầu.
    }

    void Update()
    {
        // Nếu chưa có dữ liệu level hoặc game đã xong thì không làm gì cả
        if (_currentLevelData == null || IsGameFinished) return;

        // 1. Logic Timer
        _levelTimer -= Time.deltaTime;
        if (timerText != null) timerText.text = Mathf.CeilToInt(_levelTimer).ToString();

        // 2. Kiểm tra điều kiện KẾT THÚC (Hết giờ)
        if (_levelTimer <= 0)
        {
            StartLevel(_currentLevelData);// Hết giờ -> Xong game (Thắng hay thua tùy logic bạn muốn xử lý thêm)
            return;
        }

        // 3. Logic Spawn Note
        _nextSpawnTimer -= Time.deltaTime;
        if (_nextSpawnTimer <= 0)
        {
            SpawnNote();
            _nextSpawnTimer = Random.Range(_currentLevelData.minSpawnDelay, _currentLevelData.maxSpawnDelay);
        }
    }

    // --- HÀM MỚI ĐỂ GỌI TỪ GAME FLOW MANAGER ---
    public void StartLevel(LevelData data)
    {
        if (data == null)
        {
            
            Debug.LogError("LevelData truyền vào bị null!");
            // Set luôn là true để GameFlow không bị treo mãi mãi
            IsGameFinished = true; 
            return;
        }

        _currentLevelData = data;
        AudioManager.Instance.PlayMusic((_currentLevelData.levelName));
        // Reset trạng thái game
        IsGameFinished = false;
        _levelTimer = _currentLevelData.levelDuration;
        _currentProgress = 0f;
        _lastNotePosition = Vector2.zero;
        
        // Reset UI
        if (progressSlider != null) progressSlider.value = 0f;

        // Xóa sạch các nốt nhạc cũ còn sót lại (nếu có)
        ClearAllNotes();

        Debug.Log($"Đã bắt đầu Level: {_currentLevelData.name}");
    }
    // -------------------------------------------

    public void AddProgress()
    {
        if (IsGameFinished) return;

        _currentProgress += _currentLevelData.progressPerHit;
        if (progressSlider != null) progressSlider.value = _currentProgress / 100f;

        // Kiểm tra điều kiện THẮNG
        if (_currentProgress >= 100f)
        {
            Debug.Log("Level Complete!");
            FinishGame();
        }
    }

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
        foreach (var note in activeNotes)
        {
            Destroy(note.gameObject);
        }
    }

    // ... (Giữ nguyên logic SpawnNote và ClampToCameraView cũ của bạn ở dưới) ...
    private void SpawnNote()
    {
        Vector2 rawPos = GetRelativeRandomPosition();
        Vector2 finalPos = ClampToCameraView(rawPos);

        GameObject go = Instantiate(_currentLevelData.notePrefab, finalPos, Quaternion.identity);
        
        // Nếu Note Prefab là UI Image thì cần gán vào Canvas, nếu là Sprite thì kệ
        // go.transform.SetParent(canvasTransform); // Uncomment nếu cần

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