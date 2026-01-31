using System.Collections;
using UnityEngine;
using TMPro;
using System; 

public class DialogueController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text NPCName; // Ô hiển thị tên NPC
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private float typingSpeed = 0.03f; 
    
    private DialogueData _currentData; 
    private int currentIndex = 0;
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

    public void StartDialogue(DialogueData data)
    {
        if (data == null) return;

        _currentData = data; 
        currentIndex = 0;
        dialoguePanel.SetActive(true);

        // THÊM: Cập nhật tên NPC ngay khi bắt đầu hội thoại
        if (NPCName != null)
        {
            // Giả sử trong DialogueData của bạn biến lưu tên là NPCId hoặc NPCName
            NPCName.text = _currentData.NPCName; 
        }

        ShowDialogue();
    }

    private void ShowDialogue()
    {
        if (_currentData != null && currentIndex < _currentData.dialogueLines.Count)
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
        OnDialogueFinished?.Invoke();
    }
}