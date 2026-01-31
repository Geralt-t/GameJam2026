using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Trong GameFlowManager.cs
public class GameFlowManager : MonoBehaviour
{
    // Chỉ cần 1 list này là đủ (thay vì list NPC riêng, list DialogueData riêng)
    public List<NPCController> npcList; 
    
    public LevelManager levelManager;

    void Start()
    {
        StartCoroutine(MainGameLoop());
    }

    System.Collections.IEnumerator MainGameLoop()
    {
        foreach (var npc in npcList)
        {
            // 1. Hiện NPC
            npc.SetVisualActive(true);

            // 2. Chạy hội thoại (Gọi thẳng vào hàm của NPCController)
            bool dialogueDone = false;
            npc.StartMyDialogue(() => dialogueDone = true);
            yield return new WaitUntil(() => dialogueDone);

            // 3. Xử lý Mask (Lấy tham chiếu Mask từ chính NPC đó)
            if (npc.maskObject != null)
            {
                npc.maskObject.gameObject.SetActive(true);
                npc.maskObject.EnableInteraction();
                
                bool maskBroken = false;
                System.Action onBroken = null;
                onBroken = () => { maskBroken = true; npc.maskObject.OnMaskBroken -= onBroken; };
                npc.maskObject.OnMaskBroken += onBroken;

                yield return new WaitUntil(() => maskBroken);
            }

            // Trong GameFlowManager.cs -> MainGameLoop()

// 4. Chạy Game Level
            if (npc.levelData != null)
            {
                // Gọi hàm mới vừa viết
                levelManager.StartLevel(npc.levelData);
            }
            else
            {
                Debug.LogWarning($"NPC {npc.name} chưa được gán LevelData!");
                // Nếu quên gán Data thì phải skip qua bước đợi game, nếu không sẽ bị kẹt vĩnh viễn
                // levelManager.ForceFinish(); // (Tùy chọn: viết thêm hàm này nếu cần)
            }

// Chờ game xong... (LevelManager sẽ set IsGameFinished = true khi hết giờ hoặc đủ điểm)
            yield return new WaitUntil(() => levelManager.IsGameFinished);

            // 5. Kết thúc lượt
            if (npc.maskObject != null) npc.maskObject.HealMask();
            npc.SetVisualActive(false);
            
            yield return new WaitForSeconds(1f);
        }
    }
}