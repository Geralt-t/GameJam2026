using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("NPC Specific Data")]
    public DialogueData npcDialogue; 
    public LevelData levelData;

    [Header("Mask Visuals")]
    // KÉO ẢNH MẶT NẠ CỦA NPC NÀY VÀO ĐÂY
    public Sprite maskWholeSprite;  
    public Sprite maskBrokenSprite; 

    [Header("Scene References")]
    public MaskObject maskObject;
    public DialogueController dialogueController;
    public GameObject npcVisuals;

    // Hàm cài đặt Mask trước khi hiện lên
    public void InitializeMask()
    {
        if (maskObject != null && maskWholeSprite != null)
        {
            // Nạp ảnh của NPC này vào MaskObject chung trong Scene
            maskObject.SetupMask(maskWholeSprite, maskBrokenSprite);
        }
    }

    public void StartMyDialogue(System.Action onDialogueFinished)
    {
        if (dialogueController != null && npcDialogue != null)
        {
            dialogueController.OnDialogueFinished = onDialogueFinished;
            dialogueController.StartDialogue(npcDialogue); 
        }
        else
        {
            onDialogueFinished?.Invoke();
        }
    }

    public void SetVisualActive(bool isActive)
    {
        if (npcVisuals != null) npcVisuals.SetActive(isActive);
        else gameObject.SetActive(isActive);
    }
}