using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.U2D;

[CreateAssetMenu(fileName = "ChatData", menuName = "GJPlus/ScriptableObjects/ChatData", order = 1)]
public class SOChatData : ScriptableObject
{
    [FoldoutGroup("$chatTittle")] public string chatTittle;
    [FoldoutGroup("$chatTittle")] public string charName;
    [FoldoutGroup("$chatTittle")] public Sprite chatspr;
    [FoldoutGroup("$chatTittle")][TextArea(25, 25)] public string chatData;
}
