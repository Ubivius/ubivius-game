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

public class TextChatUI : MonoBehaviour
{
    private RectTransform m_rectTransform;

    private EventSystem system;

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

    private string m_friendRegex = @"^(\/w|\/W)\s\""([a-zA-Z0-9 _]+)\""\s(.+)";

    private bool m_textChatIsActive;
    public bool m_textChatIsExpanded { get; set; }
    public bool m_textChatIsHidden { get; set; }

    // Start is called before the first frame update
    void Start() {
        m_textChatIsExpanded = false;
        m_textChatIsActive = true;
        m_textChatIsHidden = false;
        ToggleDisplayChat();

        system = EventSystem.current;
        m_rectTransform = transform.GetComponent<RectTransform>();
        m_scrollbar.value = 0;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Return) && system.currentSelectedGameObject != m_messageInputField.gameObject) {
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
            system.SetSelectedGameObject(null);
        }
    }

    public void ToggleDisplayChat() {
        m_textChatIsActive = !m_textChatIsActive;

        if (m_textChatIsExpanded && !m_textChatIsHidden)
            m_shrinkButton.gameObject.SetActive(m_textChatIsActive);
        else
            m_expandButton.gameObject.SetActive(m_textChatIsActive);

        var tmpColor = m_chatBox.GetComponent<Image>().color;
        
        if (m_textChatIsActive) {
            tmpColor.a = 0.4f;
            m_chatBox.GetComponent<Image>().color = tmpColor;
        }
        else {
            tmpColor.a = 0f;
            m_chatBox.GetComponent<Image>().color = tmpColor;
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
        m_textChatIsHidden = true;

        m_hideButton.gameObject.SetActive(false);
        m_showButton.gameObject.SetActive(true);
        m_expandButton.gameObject.SetActive(false);
        m_shrinkButton.gameObject.SetActive(false);
        m_chatBox.SetActive(false);
    }

    public void Show() {
        m_textChatIsHidden = false;

        m_chatBox.SetActive(true);
        m_hideButton.gameObject.SetActive(true);
        m_showButton.gameObject.SetActive(false);

        if (m_textChatIsExpanded)
            m_shrinkButton.gameObject.SetActive(true);
        else
            m_expandButton.gameObject.SetActive(true);
    }

    public void Send() {
        if (!string.IsNullOrEmpty(m_messageInputField.text)) {
            string tmpName = "Jean-Maurice";
            GameObject t_newMessage = Instantiate(m_messagePrefab, m_messagesBox.transform);

            // If match the friend regex, it's a private message
            Debug.Log(m_messageInputField.text);
            Match m = Regex.Match(m_messageInputField.text, m_friendRegex);
            if (m.Success) {
                t_newMessage.name = "Private Message - <INSERT ID HERE>";
                t_newMessage.GetComponent<Text>().text = $"[{DateTime.Now.Hour}:{DateTime.Now.Minute}] {tmpName} -> {m.Groups[2]} : {m.Groups[3]}";
                t_newMessage.GetComponent<Text>().color = m_privateColor;
            }
            else {
                t_newMessage.name = "Message - <INSERT ID HERE>";

                t_newMessage.GetComponent<Text>().text = $"[{DateTime.Now.Hour}:{DateTime.Now.Minute}] {tmpName} : {m_messageInputField.text}";
                t_newMessage.GetComponent<Text>().color = m_defaultColor;
            }

            m_messageInputField.text = "";
            m_scrollbar.value = 0;
        }
    }
}
