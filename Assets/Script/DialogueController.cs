using System.Collections;
using UnityEngine;
using TMPro;
using System; 

public class DialogueController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text NPCName; 
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private float typingSpeed = 0.03f; 
    
    private DialogueData _currentData; 
    private int currentIndex = 0;
    private int stopIndex = 0; // BIẾN MỚI: Điểm dừng của hội thoại
    private bool isTyping = false;          
    private bool skipTyping = false;      
    
    private Coroutine typingCoroutine;
    public Action OnDialogueFinished; 

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (dialoguePanel.activeSelf)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
            {
                NextDialogue();
            }
        }
    }

    // Hàm mặc định: Chạy toàn bộ hội thoại (Dùng cho Boss)
    public void StartDialogue(DialogueData data)
    {
        if (data == null) return;
        StartDialogueRange(data, 0, data.dialogueLines.Count - 1);
    }

    // HÀM MỚI: Cho phép chạy hội thoại từ dòng A đến dòng B
    public void StartDialogueRange(DialogueData data, int startLine, int endLine)
    {
        if (data == null || data.dialogueLines.Count == 0) return;

        _currentData = data; 
        currentIndex = startLine;
        stopIndex = endLine; // Thiết lập điểm dừng
        
        dialoguePanel.SetActive(true);

        if (NPCName != null)
        {
            NPCName.text = _currentData.NPCName; 
        }

        ShowDialogue();
    }

    private void ShowDialogue()
    {
        // SỬA ĐIỀU KIỆN: Chỉ chạy nếu currentIndex chưa vượt quá stopIndex
        if (_currentData != null && currentIndex <= stopIndex && currentIndex < _currentData.dialogueLines.Count)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(_currentData.dialogueLines[currentIndex]));
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeText(string line)
    {
        isTyping = true;
        skipTyping = false;
        dialogueText.text = "";

        foreach (char c in line.ToCharArray())
        {
            if (skipTyping)
            {
                dialogueText.text = line;
                break;
            }
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    public void NextDialogue()
    {
        if (isTyping)
        {
            skipTyping = true;
            return;
        }
        
        currentIndex++;
        ShowDialogue();
    }

    public void EndDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialoguePanel.SetActive(false);
        isTyping = false;
        
        // Gọi sự kiện để GameFlowManager biết mà chạy tiếp
        OnDialogueFinished?.Invoke();
    }
}