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
    public class TextChatUI : MonoBehaviour
    {
        private RectTransform m_rectTransform;

        private EventSystem m_system;

        [SerializeField] private GameObject m_messagesBox;
        [SerializeField] private GameObject m_chatBox;
        [SerializeField] private Scrollbar m_scrollbar;
        [SerializeField] private InputField m_messageInputField;
        [SerializeField] private GameObject m_messagePrefab;
        [SerializeField] private EventSystem m_eventSystem;
        [SerializeField] private Button m_sendButton;
        [SerializeField] private Button m_showButton;
        [SerializeField] private Button m_hideButton;
        [SerializeField] private Button m_shrinkButton;
        [SerializeField] private Button m_expandButton;

        [SerializeField] private Color m_privateColor;
        [SerializeField] private Color m_defaultColor;
        [SerializeField] private Color m_ErrorColor;

        private string m_playerName;

        private const string m_friendRegex = @"^(\/w|\/W)\s\""([a-zA-Z0-9 _]+)\""\s(.+)";
        private const string m_invalidRegex = @"^(\/\S+)";

        private bool m_textChatIsActive;
        public bool m_textChatIsExpanded { get; set; }
        public bool m_textChatIsHidden { get; set; }

        void Start() {
            m_textChatIsExpanded = false;
            m_textChatIsActive = true;
            m_textChatIsHidden = false;
            ToggleDisplayChat();

            m_system = EventSystem.current;
            m_rectTransform = transform.GetComponent<RectTransform>();
            m_scrollbar.value = 0;

            // TODO Get the current player to be use in send method to send data to the microservice
            m_playerName = "Jean-Maurice"; // Placeholder name
        }

        void Update() {
            // Toggle the chat display by pressing return key
            if (Input.GetKeyDown(KeyCode.Return) &&
                m_system.currentSelectedGameObject != m_messageInputField.gameObject) {
                if (m_textChatIsHidden)
                    Show();

                ToggleDisplayChat();
                m_messageInputField.Select();
                m_messageInputField.ActivateInputField();
            }
            else if (Input.GetKeyDown(KeyCode.Return)) {
                ToggleDisplayChat();
                Send();
                m_messageInputField.DeactivateInputField();
                m_system.SetSelectedGameObject(null);
            }

            // TODO Check if new message in the conversation and print them with PrintGeneralMessage() / PrintPrivateMessage()
        }

        public void ToggleDisplayChat() {
            m_textChatIsActive = !m_textChatIsActive;

            if (m_textChatIsExpanded && !m_textChatIsHidden)
                m_shrinkButton.gameObject.SetActive(m_textChatIsActive);
            else
                m_expandButton.gameObject.SetActive(m_textChatIsActive);

            var t_color = m_chatBox.GetComponent<Image>().color;

            if (m_textChatIsActive) {
                t_color.a = 0.4f;
                m_chatBox.GetComponent<Image>().color = t_color;
            }
            else {
                t_color.a = 0f;
                m_chatBox.GetComponent<Image>().color = t_color;
            }

            m_messageInputField.gameObject.SetActive(m_textChatIsActive);
            m_sendButton.gameObject.SetActive(m_textChatIsActive);

            if (m_textChatIsHidden)
                m_showButton.gameObject.SetActive(m_textChatIsActive);
            else
                m_hideButton.gameObject.SetActive(m_textChatIsActive);

        }

        public void Expand() {
            m_textChatIsExpanded = true;

            m_rectTransform.offsetMax = new Vector2(m_rectTransform.offsetMax.x, -10);

            m_expandButton.gameObject.SetActive(false);
            m_shrinkButton.gameObject.SetActive(true);
        }

        public void Shrink() {
            m_textChatIsExpanded = false;

            m_rectTransform.offsetMax = new Vector2(m_rectTransform.offsetMax.x, -820);

            m_expandButton.gameObject.SetActive(true);
            m_shrinkButton.gameObject.SetActive(false);
        }

        public void Hide() {
            m_chatBox.SetActive(false);
            m_textChatIsHidden = true;

            m_hideButton.gameObject.SetActive(false);
            m_showButton.gameObject.SetActive(true);

            m_expandButton.gameObject.SetActive(false);
            m_shrinkButton.gameObject.SetActive(false);
        }

        public void Show() {
            m_chatBox.SetActive(true);
            m_textChatIsHidden = false;

            m_hideButton.gameObject.SetActive(true);
            m_showButton.gameObject.SetActive(false);

            if (m_textChatIsExpanded)
                m_shrinkButton.gameObject.SetActive(true);
            else
                m_expandButton.gameObject.SetActive(true);
        }

        public void Send() {
            if (!string.IsNullOrEmpty(m_messageInputField.text)) {

                Match t_matchFriend = Regex.Match(m_messageInputField.text, m_friendRegex);
                Match t_matchInvalidCommand = Regex.Match(m_messageInputField.text, m_invalidRegex);

                if (t_matchFriend.Success) {
                    PrintPrivateMessage(m_playerName, t_matchFriend.Groups[2].Value, t_matchFriend.Groups[3].Value);

                    // TODO Send message to microservice
                }
                else if (t_matchInvalidCommand.Success) {
                    PrintInvalidMessage(t_matchInvalidCommand.Groups[0].Value);
                }
                else {
                    PrintGeneralMessage(m_playerName, m_messageInputField.text);

                    // TODO Send message to microservice
                }

                m_messageInputField.text = "";
                m_scrollbar.value = 0;
            }
        }

        private void PrintPrivateMessage(string a_sender, string a_receiver, string a_message) {
            GameObject t_newMessage = Instantiate(m_messagePrefab, m_messagesBox.transform);
            t_newMessage.name = $"Private Message - {a_sender}";
            t_newMessage.GetComponent<Text>().text =
                $"[{DateTime.Now.Hour}:{DateTime.Now.Minute}] {a_sender} -> {a_receiver} : {a_message}";
            t_newMessage.GetComponent<Text>().color = m_privateColor;
        }

        private void PrintGeneralMessage(string a_sender, string a_message) {
            GameObject t_newMessage = Instantiate(m_messagePrefab, m_messagesBox.transform);
            t_newMessage.name = $"Message - {a_sender}";
            t_newMessage.GetComponent<Text>().text =
                $"[{DateTime.Now.Hour}:{DateTime.Now.Minute}] {a_sender} : {a_message}";
            t_newMessage.GetComponent<Text>().color = m_defaultColor;
        }

        private void PrintInvalidMessage(string a_invalidCommand) {
            GameObject t_newMessage = Instantiate(m_messagePrefab, m_messagesBox.transform);
            t_newMessage.name = $"Error Message";
            t_newMessage.GetComponent<Text>().text =
                $"[{DateTime.Now.Hour}:{DateTime.Now.Minute}] The command {a_invalidCommand} isn't valid.";
            t_newMessage.GetComponent<Text>().color = m_ErrorColor;
        }
    }
}
