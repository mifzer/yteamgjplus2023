using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


[CreateAssetMenu(fileName = "ChatData", menuName = "GJPlus/ScriptableObjects/ConversationData", order = 0)]
public class SOConversationData : ScriptableObject
{
    [FoldoutGroup("$ConversationTittle")] public string ConversationTittle;
    [FoldoutGroup("$ConversationTittle")] public List<SOChatData> listChatData;
}
