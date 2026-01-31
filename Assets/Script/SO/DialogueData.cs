using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "ScriptableObject/DialogueData")]
public class DialogueData : ScriptableObject
{
    public string NPCId;
    [TextArea(3, 10)]
    public List<string> dialogueLines;
}