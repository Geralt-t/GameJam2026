using System.Collections;
using UnityEngine;
using TMPro;
using System; 

public class DialogueController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private float typingSpeed = 0.03f; 
    
    private DialogueData _currentData; // Biến lưu data được truyền vào
    private int currentIndex = 0;
    private bool isTyping = false;          
    private bool skipTyping = false;      
    
    private Coroutine typingCoroutine;
    
    // Action để báo về cho NPCController biết là đã xong
    public Action OnDialogueFinished; 

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        // Logic bấm chuột để next
        if (dialoguePanel.activeSelf)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                NextDialogue();
            }
        }
    }

    // --- SỬA ĐỔI CHÍNH TẠI ĐÂY ---
    // Hàm này bây giờ nhận vào DialogueData
    public void StartDialogue(DialogueData data)
    {
        if (data == null)
        {
            Debug.LogError("Không có DialogueData được truyền vào!");
            EndDialogue();
            return;
        }

        _currentData = data; // Lưu lại data của NPC hiện tại
        currentIndex = 0;
        
        dialoguePanel.SetActive(true);
        ShowDialogue();
    }
    // -----------------------------

    private void ShowDialogue()
    {
        if (currentIndex < _currentData.dialogueLines.Count)
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
        
        // Gọi Action để báo cho NPCController biết
        OnDialogueFinished?.Invoke();
    }
}