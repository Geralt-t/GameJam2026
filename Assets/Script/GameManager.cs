using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject hitObjectPrefab;
    public List<HitObjectData> beatmap = new List<HitObjectData>();
    [SerializeField] private Queue<HitObjectData> _waitingQueue = new Queue<HitObjectData>();
    [SerializeField] private Dictionary<KeyCode, bool> _activeLanes = new Dictionary<KeyCode, bool>();
    private float _spawnTimer = 0f;
    private bool _canSpawnNext = true;
    void Start()
    {
        _activeLanes[KeyCode.Q] = false;
        _activeLanes[KeyCode.W] = false;
        _activeLanes[KeyCode.E] = false;
        _activeLanes[KeyCode.R] = false;

        AddNote(KeyCode.Q, 5.0f, new Vector2(-2,0));
        AddNote(KeyCode.W, 2.0f, new Vector2(-4,0));
        AddNote(KeyCode.Q, 2.0f, new Vector2(-2,2));
        AddNote(KeyCode.E, 2.0f, new Vector2(1,2));
    }

    private void AddNote(KeyCode key, float duration, Vector2 pos)
    {
        _waitingQueue.Enqueue(new HitObjectData { hitKey = key, hitTime = duration, position = pos });
    }

    void Update()
    {
        if (_waitingQueue.Count == 0) return;

        if(_spawnTimer > 0f)
        {
            _spawnTimer -= Time.deltaTime;
        }
        HitObjectData nextNode =  _waitingQueue.Peek();

        bool isLaneActive = _activeLanes[nextNode.hitKey];

        bool isTimeReady = _spawnTimer <=0;

        if (!isLaneActive & isTimeReady)
        {
            Spawn(_waitingQueue.Dequeue());
        }
    }

    private void Spawn(HitObjectData hitObjectData)
    {
        GameObject go = Instantiate(hitObjectPrefab);
        HitObject hitObject = go.GetComponent<HitObject>();
        hitObject.Initialize(hitObjectData);

        hitObject.OnObjectDestroyed += HandleNoteDestroyed;

        _activeLanes[hitObjectData.hitKey] = true;
        _spawnTimer = hitObjectData.hitTime / 2f;
    }

    private void HandleNoteDestroyed(KeyCode key)
    {
        _activeLanes[key] = false;
    }
}
