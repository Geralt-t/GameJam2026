using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("NPC Specific Data")]
    // Dữ liệu hội thoại riêng của NPC này (ScriptableObject)
    public DialogueData npcDialogue; 
    
    // Dữ liệu màn chơi Rhythm Game riêng của NPC này
    public LevelData levelData;

    [Header("Scene References")]
    // Tham chiếu đến MaskObject trong Scene (nếu mỗi NPC dùng chung 1 mask thì kéo mask đó vào, nếu riêng thì kéo mask riêng)
    public MaskObject maskObject;
    
    // Tham chiếu đến DialogueController trong Scene
    public DialogueController dialogueController;

    [Header("Visuals")]
    public GameObject npcVisuals; // Sprite hoặc Model của NPC

    // --- CÁC HÀM HỖ TRỢ ĐỂ MANAGER GỌI DỄ DÀNG ---

    // 1. Hàm bắt đầu hội thoại của chính NPC này
    public void StartMyDialogue(System.Action onDialogueFinished)
    {
        if (dialogueController != null && npcDialogue != null)
        {
            // Gán sự kiện callback
            dialogueController.OnDialogueFinished = onDialogueFinished;
            
            // --- SỬA DÒNG NÀY ---
            // Truyền npcDialogue vào trong hàm StartDialogue
            dialogueController.StartDialogue(npcDialogue); 
            // --------------------
        }
        else
        {
            Debug.LogWarning($"NPC {name} thiếu dữ liệu hoặc controller!");
            onDialogueFinished?.Invoke();
        }
    }

    // 2. Hàm bật/tắt hiển thị NPC
    public void SetVisualActive(bool isActive)
    {
        if (npcVisuals != null)
        {
            npcVisuals.SetActive(isActive);
        }
        else
        {
            gameObject.SetActive(isActive);
        }
    }
}