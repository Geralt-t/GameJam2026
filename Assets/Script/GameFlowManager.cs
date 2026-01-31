using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameFlowManager : MonoBehaviour
{
    [Header("Boss Settings")]
    public BossNPCController bossController; 

    [Header("NPC List")]
    public List<NPCController> npcList; 
    
    [Header("System References")]
    public LevelManager levelManager;
    public GameObject backGroundGameUI;

    void Start()
    {
        AudioManager.Instance.PlayMusic("default");
        
        if (bossController != null) bossController.SetVisualActive(false);
        foreach (var npc in npcList) npc.SetVisualActive(false);

        StartCoroutine(MainGameLoop());
    }

    IEnumerator MainGameLoop()
    {
        // --- PHẦN 1: ÔNG CHỦ (Giữ nguyên - Boss vẫn nói một lèo) ---
        if (bossController != null)
        {
            Debug.Log("Ông chủ xuất hiện...");
            bossController.SetVisualActive(true);
            bool bossDone = false;
            bossController.StartBossDialogue(() => bossDone = true);
            yield return new WaitUntil(() => bossDone);
            bossController.SetVisualActive(false);
            yield return new WaitForSeconds(1.0f);
        }

        // --- PHẦN 2: CÁC NPC ---
        foreach (var npc in npcList)
        {
            // BƯỚC 1: Hiện NPC và Chạy Hội Thoại (PHẦN ĐẦU)
            npc.SetVisualActive(true);
            
            // Lấy tổng số dòng thoại
            int totalLines = 0;
            if (npc.npcDialogue != null) totalLines = npc.npcDialogue.dialogueLines.Count;

            bool dialoguePart1Done = false;

            // LOGIC CHIA HỘI THOẠI
            if (totalLines > 1)
            {
                // Nếu có nhiều hơn 1 dòng -> Nói từ đầu đến (cuối - 1)
                // Cần truy cập trực tiếp vào dialogueController của NPC để gọi hàm Range
                if (npc.dialogueController != null)
                {
                    npc.dialogueController.OnDialogueFinished = () => dialoguePart1Done = true;
                    // Chạy từ 0 đến kế cuối
                    npc.dialogueController.StartDialogueRange(npc.npcDialogue, 0, totalLines - 2);
                }
                else dialoguePart1Done = true; // Phòng lỗi
            }
            else
            {
                // Nếu chỉ có 1 dòng hoặc ít hơn -> Nói hết luôn ở đây (hoặc tùy bạn chỉnh)
                npc.StartMyDialogue(() => dialoguePart1Done = true);
            }
            
            yield return new WaitUntil(() => dialoguePart1Done);

            // BƯỚC 2: Hiện Mask Vỡ và CHỜ CLICK (Giữ nguyên)
            if (npc.maskObject != null)
            {
                npc.InitializeMask(); 
                npc.maskObject.gameObject.SetActive(true);
                npc.maskObject.spriteRenderer.sprite = npc.maskBrokenSprite;
                npc.maskObject.ShowMaskAnimated();

                Debug.Log("Đã hiện Mask, chờ click...");

                bool isMaskClicked = false;
                System.Action onMaskClick = null;
                onMaskClick = () => { 
                    isMaskClicked = true; 
                    npc.maskObject.OnMaskClicked -= onMaskClick; 
                };
                npc.maskObject.OnMaskClicked += onMaskClick;

                yield return new WaitUntil(() => isMaskClicked);
                yield return new WaitForSeconds(0.5f);
            }

            // BƯỚC 3: Vào Game Puzzle (Giữ nguyên)
            if (npc.levelData != null)
            {
                if(npc.maskObject != null) npc.maskObject.gameObject.SetActive(false);

                backGroundGameUI.SetActive(true);
                levelManager.StartLevel(npc.levelData);
                
                yield return new WaitUntil(() => levelManager.IsGameFinished);
            }

            // BƯỚC 4: Thắng game -> Hiện lại Mask Lành & NÓI CÂU CUỐI
            if (npc.maskObject != null)
            {
                npc.maskObject.gameObject.SetActive(true);
                npc.maskObject.HealMask(); 
                Debug.Log("Game Win! Mặt nạ lành.");
                backGroundGameUI.SetActive(false);
                // --- LOGIC MỚI: CHẠY CÂU THOẠI CUỐI CÙNG ---
                if (totalLines > 1)
                {
                    // Hiện lại NPC Visual (nếu nãy bị che bởi mask hoặc game)
                    npc.SetVisualActive(true);

                    bool dialoguePart2Done = false;
                    
                    if (npc.dialogueController != null)
                    {
                        npc.dialogueController.OnDialogueFinished = () => dialoguePart2Done = true;
                        // Chỉ chạy đúng dòng cuối cùng (index = totalLines - 1)
                        npc.dialogueController.StartDialogueRange(npc.npcDialogue, totalLines - 1, totalLines - 1);
                    }
                    else dialoguePart2Done = true;

                    // Chờ nói xong câu cuối mới cho qua
                    yield return new WaitUntil(() => dialoguePart2Done);
                }
                // ---------------------------------------------

                // Chờ thêm 1 chút cho người chơi cảm nhận
                yield return new WaitForSeconds(1.5f);
                
                // Dọn dẹp
                npc.maskObject.gameObject.SetActive(false);
            }
            
            backGroundGameUI.SetActive(false);
            npc.SetVisualActive(false);
            yield return new WaitForSeconds(1f);
        }
    }
}