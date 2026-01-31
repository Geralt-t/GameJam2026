using System.Collections;
using UnityEngine;

using TMPro;
public class DialogueController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private float minWaitTime = 2.0f;
    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.03f; 
    
    private float timerSinceStartedLine = 0f;
    private int currentIndex = 0;
    private bool isTyping = false;          
    private bool skipTyping = false;      
    public bool IsDialogueActive { get; private set; }
    private bool isFinishDialogue=false;
    private Coroutine typingCoroutine;
    public System.Action OnDialogueFinished;

    private void Start()
    {
        dialoguePanel.SetActive(false);
        IsDialogueActive = false;
    }
    private void Update()
    {
        if (IsDialogueActive)
        {
            timerSinceStartedLine += Time.deltaTime;
        }
    }
    public void StartDialogue()
    {
        currentIndex = 0;
        dialoguePanel.SetActive(true);
        IsDialogueActive = true;
        ShowDialogue();
        isFinishDialogue = false;
    }

    private void ShowDialogue()
    {
        if (currentIndex < dialogueData.dialogueLines.Count)
        {
            timerSinceStartedLine = 0f;
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            string voiceClipName = dialogueData.NPCId + currentIndex;
            // if (AudioManager.Instance != null)
            // {
            //     AudioManager.Instance.PlaySFX(voiceClipName, 1f, false);
            // }
            typingCoroutine = StartCoroutine(TypeText(dialogueData.dialogueLines[currentIndex]));
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
                dialogueText.text = line; // hi?n th? h?t lu�n
                break;
            }

            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void NextDialogue()
    {
        if (timerSinceStartedLine < minWaitTime)
        {
            return;
        }
        if (isTyping)
        {
            skipTyping = true;
            string voiceClipName = dialogueData.NPCId + currentIndex;
            // if (AudioManager.Instance != null)
            // {
            //     AudioManager.Instance.StopSFX(voiceClipName);
            // }
            return;
        }

        currentIndex++;
        ShowDialogue();
    }

    public void EndDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        string voiceClipName = dialogueData.NPCId + (currentIndex - 1);;
        // if (AudioManager.Instance != null)
        // {
        //     AudioManager.Instance.StopSFX(voiceClipName);
        // }
        dialoguePanel.SetActive(false);
        IsDialogueActive = false;
        currentIndex = 0;
        isTyping = false;
        skipTyping = false;
        isFinishDialogue = true;
        OnDialogueFinished?.Invoke();

    }
    public bool isFinishedDialogue()
    {
        if (isFinishDialogue)
        {
            return true;
        }
        return false;
    }
}
