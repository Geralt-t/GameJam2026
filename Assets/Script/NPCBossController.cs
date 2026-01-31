using UnityEngine;

public class BossNPCController : MonoBehaviour
{
    [Header("Data")]
    public DialogueData bossDialogue; // Chỉ cần dữ liệu hội thoại

    [Header("References")]
    public DialogueController dialogueController; // UI điều khiển thoại
    public GameObject bossVisuals; // Sprite của ông chủ

    // Hàm bắt đầu hội thoại (Giống NPCController nhưng gọn hơn)
    public void StartBossDialogue(System.Action onDialogueFinished)
    {
        // Kiểm tra an toàn
        if (dialogueController == null)
        {
            Debug.LogError($"Boss '{name}' chưa gán DialogueController!");
            onDialogueFinished?.Invoke();
            return;
        }

        if (bossDialogue == null)
        {
            Debug.LogError($"Boss '{name}' chưa gán DialogueData!");
            onDialogueFinished?.Invoke();
            return;
        }

        // Gán sự kiện khi thoại xong thì báo lại cho Manager
        dialogueController.OnDialogueFinished = onDialogueFinished;
        
        // Bắt đầu chạy thoại
        dialogueController.StartDialogue(bossDialogue);
    }

    // Hàm bật tắt hình ảnh
    public void SetVisualActive(bool isActive)
    {
        if (bossVisuals != null)
        {
            bossVisuals.SetActive(isActive);
        }
        else
        {
            // Nếu không gán visuals riêng thì bật tắt chính GameObject này
            gameObject.SetActive(isActive);
        }
    }
}