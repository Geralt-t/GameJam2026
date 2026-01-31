using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameFlowManager : MonoBehaviour
{
    [Header("Boss Settings")]
    public BossNPCController bossController; 

    [Header("NPC List")]
    public List<NPCController> npcList; // Các NPC thường vẫn dùng script cũ
    
    [Header("System References")]
    public LevelManager levelManager;

    public GameObject backGroundGameUI;

    void Start()
    {
        // Tắt visual ban đầu
        if (bossController != null) bossController.SetVisualActive(false);
        foreach (var npc in npcList) npc.SetVisualActive(false);

        StartCoroutine(MainGameLoop());
    }

    IEnumerator MainGameLoop()
    {
        // --- PHẦN 1: ÔNG CHỦ ---
        if (bossController != null)
        {
            Debug.Log("Ông chủ xuất hiện...");
            bossController.SetVisualActive(true);

            bool bossDone = false;
            
            // Gọi hàm bên script mới
            bossController.StartBossDialogue(() => bossDone = true);
            
            yield return new WaitUntil(() => bossDone);

            bossController.SetVisualActive(false);
            yield return new WaitForSeconds(1.0f);
        }

        // --- PHẦN 2: CÁC NPC KHÁC (Giữ nguyên) ---
        foreach (var npc in npcList)
        {
            // 1. Hiện NPC
            npc.SetVisualActive(true);
            
            // 2. LOGIC MỚI: Hiện luôn mặt nạ ở trạng thái VỠ ngay từ đầu
            if (npc.maskObject != null)
            {
                npc.InitializeMask(); 
                npc.maskObject.gameObject.SetActive(true);
                
                // ÉP MẶT NẠ HIỆN SPRITE VỠ NGAY LẬP TỨC
                // Giả sử bạn đã có hàm SetupMask trong MaskObject, 
                // ta sẽ gán thẳng sprite vỡ vào renderer.
                npc.maskObject.spriteRenderer.sprite = npc.maskBrokenSprite;
            }

            // 3. Chạy hội thoại của NPC
            bool dialogueDone = false;
            npc.StartMyDialogue(() => dialogueDone = true);
            yield return new WaitUntil(() => dialogueDone);

            // 4. Chơi Game Nhạc (Mặt nạ vỡ vẫn nằm đó làm nền hoặc tùy bạn ẩn/hiện)
            if (npc.levelData != null)
            {
                npc.maskObject.gameObject.SetActive(false);
                backGroundGameUI.SetActive(true);
                levelManager.StartLevel(npc.levelData);
                yield return new WaitUntil(() => levelManager.IsGameFinished);
            }

            // 5. LOGIC MỚI: KHI THẮNG GAME -> HIỆN MẶT NẠ LÀNH
            if (npc.maskObject != null)
            {
                // Gọi hàm HealMask để đổi về Sprite lành
                npc.maskObject.HealMask(); 
                Debug.Log("Game Win! Mặt nạ đã lành lại.");
                
                yield return new WaitForSeconds(2.0f); // Chờ người chơi nhìn thấy mặt nạ lành
                
                // Tắt các UI game và mặt nạ để sang NPC tiếp theo
                backGroundGameUI.SetActive(false);
                npc.maskObject.gameObject.SetActive(false);
            }
            
            npc.SetVisualActive(false);
            yield return new WaitForSeconds(1f);
        }
    }
}