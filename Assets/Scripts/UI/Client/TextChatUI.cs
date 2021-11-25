using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using ubv.client.logic;
using ubv.microservices;
using UnityEngine.InputSystem.Interactions;

namespace ubv.ui.client
{
    [RequireComponent(typeof(RectTransform))]
    public class TextChatUI : MonoBehaviour
    {
        private RectTransform m_rectTransform;

        private EventSystem m_system;

        private SocialServicesController m_socialServices;

        [Header("UI components")]
        [SerializeField] private GameObject m_messagesBox;
        [SerializeField] private GameObject m_chatBox;
        [SerializeField] private Scrollbar m_scrollbar;
        [SerializeField] private InputField m_messageInputField;
        [SerializeField] private GameObject m_messagePrefab;
        [SerializeField] private Button m_sendButton;
        [SerializeField] private Button m_showButton;
        [SerializeField] private Button m_hideButton;
        [SerializeField] private Button m_shrinkButton;
        [SerializeField] private Button m_expandButton;

        [Header("Colors")]
        [SerializeField] private Color m_privateColor;
        [SerializeField] private Color m_defaultColor;
        [SerializeField] private Color m_ErrorColor;
        
        // user, text
        private Queue<Tuple<string, string, string, DateTime>> m_newPrivateMessagesQueue;
        private Queue<Tuple<string, string, DateTime>> m_newGeneralMessagesQueue;
        private Queue<string> m_errorMessageQueue;

        private const string m_friendRegex = @"^(\/w|\/W)\s\""([a-zA-Z0-9 _]+)\""\s(.+)";
        private const string m_invalidRegex = @"^(\/\S+)";

        private bool m_textChatIsActive;
        public bool TextChatIsExpanded { get; set; }
        public bool TextChatIsHidden { get; set; }

        private void Awake()
        {
            m_newPrivateMessagesQueue = new Queue<Tuple<string, string, string, DateTime>>();
            m_newGeneralMessagesQueue = new Queue<Tuple<string, string, DateTime>>();
            m_errorMessageQueue = new Queue<string>();
            m_socialServices = ClientSyncState.SocialServices;
        }

        void Start() {
            TextChatIsExpanded = false;
            m_textChatIsActive = true;
            TextChatIsHidden = true;
            ToggleDisplayChat();

            m_system = EventSystem.current;
            m_rectTransform = transform.GetComponent<RectTransform>();
            m_scrollbar.value = 0;
            
            m_socialServices.OnNewPrivateMessage += OnNewPrivateMessageFrom;
            m_socialServices.OnNewGeneralMessage += OnNewGeneralMessage;
        }

        void Update() {
            // Toggle the chat display by pressing return key
            if (Input.GetKeyDown(KeyCode.Return) &&
                m_system.currentSelectedGameObject != m_messageInputField.gameObject) {
                if (TextChatIsHidden)
                    Show();

                m_textChatIsActive = true;
                ToggleDisplayChat();
                m_messageInputField.Select();
                m_messageInputField.ActivateInputField();
            }
            else if (Input.GetKeyDown(KeyCode.Return) && m_textChatIsActive)
            {
                Send();
            }

            if (Input.GetKeyDown(KeyCode.Escape) && m_textChatIsActive)
            {
                TextChatIsHidden = true;
                ToggleDisplayChat();
                m_messageInputField.DeactivateInputField();
                m_system.SetSelectedGameObject(null);
            }

            while(m_newPrivateMessagesQueue.Count > 0)
            {
                Tuple<string, string, string, DateTime> msg = m_newPrivateMessagesQueue.Dequeue();
                PrintPrivateMessage(msg.Item1, msg.Item2, msg.Item3, msg.Item4);
            }
            while (m_newGeneralMessagesQueue.Count > 0)
            {
                Tuple<string, string, DateTime> msg = m_newGeneralMessagesQueue.Dequeue();
                PrintGeneralMessage(msg.Item1, msg.Item2, msg.Item3);
            }
            while (m_errorMessageQueue.Count > 0)
            {
                string msg = m_errorMessageQueue.Dequeue();
                PrintError(msg, DateTime.Now);
            }
        }

        public void ToggleDisplayChat() {

            m_shrinkButton.gameObject.SetActive(m_textChatIsActive && TextChatIsExpanded && !TextChatIsHidden);
            m_expandButton.gameObject.SetActive(m_textChatIsActive && !TextChatIsExpanded && !TextChatIsHidden);

            var t_color = m_chatBox.GetComponent<Image>().color;

            if (!TextChatIsHidden) {
                InputSystem.DisableDevice(Keyboard.current);
                t_color.a = 0.4f;
                m_chatBox.GetComponent<Image>().color = t_color;
            }
            else {
                InputSystem.EnableDevice(Keyboard.current);
                t_color.a = 0f;
                m_chatBox.GetComponent<Image>().color = t_color;
            }

            m_messageInputField.gameObject.SetActive(m_textChatIsActive);
            m_sendButton.gameObject.SetActive(m_textChatIsActive);

            m_showButton.gameObject.SetActive(m_textChatIsActive && TextChatIsHidden);
            m_hideButton.gameObject.SetActive(m_textChatIsActive && !TextChatIsHidden);

        }

        public void Expand() {
            TextChatIsExpanded = true;

            m_rectTransform.offsetMax = new Vector2(m_rectTransform.offsetMax.x, -10);

            m_expandButton.gameObject.SetActive(false);
            m_shrinkButton.gameObject.SetActive(true);
        }

        public void Shrink() {
            TextChatIsExpanded = false;

            m_rectTransform.offsetMax = new Vector2(m_rectTransform.offsetMax.x, -820);

            m_expandButton.gameObject.SetActive(true);
            m_shrinkButton.gameObject.SetActive(false);
        }

        public void Hide() {
            m_chatBox.SetActive(false);
            TextChatIsHidden = true;

            m_hideButton.gameObject.SetActive(false);
            m_showButton.gameObject.SetActive(true);

            m_expandButton.gameObject.SetActive(false);
            m_shrinkButton.gameObject.SetActive(false);
        }

        public void Show() {
            m_chatBox.SetActive(true);
            TextChatIsHidden = false;

            m_hideButton.gameObject.SetActive(true);
            m_showButton.gameObject.SetActive(false);

            if (TextChatIsExpanded)
                m_shrinkButton.gameObject.SetActive(true);
            else
                m_expandButton.gameObject.SetActive(true);
        }

        public void Send() {
            if (!string.IsNullOrEmpty(m_messageInputField.text)) {

                Match t_matchFriend = Regex.Match(m_messageInputField.text, m_friendRegex);
                Match t_matchInvalidCommand = Regex.Match(m_messageInputField.text, m_invalidRegex);

                if (t_matchFriend.Success)
                {
                    m_socialServices.GetFriendIDFromName(t_matchFriend.Groups[2].Value, (string id) =>
                    {
                        m_socialServices.SendMessageToUser(id, t_matchFriend.Groups[3].Value, null, (string message) => {
                            m_errorMessageQueue.Enqueue(message);
                        });
                    }, () => 
                    {
                        m_errorMessageQueue.Enqueue($"Friend { t_matchFriend.Groups[2].Value } not found");
                    });
                }
                else if (t_matchInvalidCommand.Success) {
                    PrintInvalidMessage(t_matchInvalidCommand.Groups[0].Value);
                }
                else
                {
                    m_socialServices.SendMessageToCurrentGameChat(m_messageInputField.text, null, (string message) => {
                        m_errorMessageQueue.Enqueue(message);
                    });
                }

                m_messageInputField.text = "";
                m_scrollbar.value = 0;
            }
        }

        private void OnNewPrivateMessageFrom(string senderID, string receiverID, MessageInfo msg)
        {
            m_socialServices.GetUserInfo(senderID, (UserInfo sender) => {
                m_socialServices.GetUserInfo(receiverID, (UserInfo receiver) => {
                    m_newPrivateMessagesQueue.Enqueue(new Tuple<string, string, string, DateTime>(sender.UserName, receiver.UserName, msg.Text, msg.CreatedOn));
                });
            });
        }

        private void OnNewGeneralMessage(string senderID, MessageInfo msg)
        {
            m_socialServices.GetUserInfo(senderID, (UserInfo sender) => {
                m_newGeneralMessagesQueue.Enqueue(new Tuple<string, string, DateTime>(sender.UserName, msg.Text, msg.CreatedOn));
            });
        }

        private void PrintPrivateMessage(string a_sender, string a_receiver, string a_message, DateTime timestamp) {
            GameObject t_newMessage = Instantiate(m_messagePrefab, m_messagesBox.transform);
            t_newMessage.name = $"Private Message - {a_sender}";
            t_newMessage.GetComponent<Text>().text =
                $"[{timestamp.Hour}:{timestamp.Minute}] {a_sender} -> {a_receiver} : {a_message}";
            t_newMessage.GetComponent<Text>().color = m_privateColor;
        }

        private void PrintGeneralMessage(string a_sender, string a_message, DateTime timestamp) {
            GameObject t_newMessage = Instantiate(m_messagePrefab, m_messagesBox.transform);
            t_newMessage.name = $"Message - {a_sender}";
            t_newMessage.GetComponent<Text>().text =
                $"[{timestamp.Hour}:{timestamp.Minute}] {a_sender} : {a_message}";
            t_newMessage.GetComponent<Text>().color = m_defaultColor;
        }

        private void PrintInvalidMessage(string a_invalidCommand) {
            PrintError($"The command { a_invalidCommand } isn't valid.", DateTime.Now);
        }

        private void PrintError(string errorMessage, DateTime timestamp)
        {
            GameObject t_newMessage = Instantiate(m_messagePrefab, m_messagesBox.transform);
            t_newMessage.name = $"Error Message";
            t_newMessage.GetComponent<Text>().text =
                $"[{timestamp.Hour}:{timestamp.Minute}] { errorMessage }.";
            t_newMessage.GetComponent<Text>().color = m_ErrorColor;
        }
    }
}
