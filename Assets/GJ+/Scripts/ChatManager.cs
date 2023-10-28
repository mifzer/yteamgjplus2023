using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.U2D;
using System.ComponentModel;
using UnityEditor.Rendering;

namespace GJPlus2023
{
    public class ChatManager : MonoBehaviour
    {
        [FoldoutGroup("Chat Manager")][SerializeField] private Animator animator;
        [FoldoutGroup("Chat Manager")][SerializeField] private GameObject _PanelChat;
        [FoldoutGroup("Chat Manager")][SerializeField] private Image _ImageChar;
        [FoldoutGroup("Chat Manager")][SerializeField] private TextMeshProUGUI txtNameChar, txtContentChat;
        [FoldoutGroup("Chat Manager")][SerializeField] private float _SpeedText;
        [FoldoutGroup("Chat Manager")] public List<SOConversationData> listConversationData;
        private Queue<SOChatData> sentences;
        private void Start()
        {
            sentences = new Queue<SOChatData>();
        }
        public void OpenConversation(int index)
        {
            _PanelChat.SetActive(true);
            sentences.Clear();
            animator.SetBool("isOpen", true);
            foreach (SOChatData sentence in listConversationData[index].listChatData)
            {
                sentences.Enqueue(sentence);
            }
            DisplayNextSentence();
        }
        public void DisplayNextSentence()
        {
            if (sentences.Count == 0)
            {
                EndDialogue();
                return;
            }
            SOChatData sentence = sentences.Dequeue();
            txtNameChar.text = sentence.charName;
            _ImageChar.sprite = sentence.chatspr;
            StopAllCoroutines();
            StartCoroutine(TypeSentence(sentence.chatData));
        }
        IEnumerator TypeSentence(string sentence)
        {
            txtContentChat.text = "";
            foreach (char letter in sentence.ToCharArray())
            {
                txtContentChat.text += letter;
                yield return new WaitForSeconds(_SpeedText);
            }
            DisplayNextSentence();
        }
        public void EndDialogue()
        {
            animator.SetBool("isOpen", false);
        }

    }
}