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
            // 1. Hiện NPC & Thoại
            npc.SetVisualActive(true);
            
            bool dialogueDone = false;
            npc.StartMyDialogue(() => dialogueDone = true);
            yield return new WaitUntil(() => dialogueDone);

            // 2. Hiện Mask -> Chờ phá
            if (npc.maskObject != null)
            {
                npc.InitializeMask(); // Nạp ảnh mask của NPC này
                npc.maskObject.gameObject.SetActive(true);
                npc.maskObject.EnableInteraction();
                
                bool maskBroken = true;
                System.Action onBroken = null;
                onBroken = () => { maskBroken = false; npc.maskObject.OnMaskBroken -= onBroken; };
                npc.maskObject.OnMaskBroken += onBroken;

                yield return new WaitUntil(() => maskBroken);
                npc.maskObject.gameObject.SetActive(false);
            }

            // 3. Chơi Game Nhạc
            if (npc.levelData != null)
            {
                levelManager.StartLevel(npc.levelData);
                yield return new WaitUntil(() => levelManager.IsGameFinished);
            }

            // 4. Kết thúc lượt
            if (npc.maskObject != null)
            {
                npc.maskObject.HealMask();
                yield return new WaitForSeconds(1f);
                npc.maskObject.gameObject.SetActive(false);
            }
            
            npc.SetVisualActive(false);
            yield return new WaitForSeconds(1f);
        }
    }
}