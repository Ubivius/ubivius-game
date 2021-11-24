using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ubv.microservices;

namespace ubv.ui.client
{
    [RequireComponent(typeof(RectTransform))]
    public class FriendUI : MonoBehaviour
    {
        [Header("UI components")]
        [SerializeField] private TextMeshProUGUI m_username;
        [SerializeField] private TextMeshProUGUI m_userStatus;
        [SerializeField] private Image m_userStatusDot;
        [SerializeField] private Image m_userStatusBar;

        [Header("Colors")]
        [SerializeField] private Color m_onlineColor;
        [SerializeField] private Color m_offlineColor;
        [SerializeField] private Color m_inGameColor;

        public void SetFriend(RelationInfo friend)
        {
            m_username.text = friend.FriendUsername;
            SetStatus(friend.FriendStatus);
        }

        public void SetStatus(StatusType status)
        {
            switch (status)
            {
                case StatusType.Offline:
                    m_userStatus.text = "Hors Ligne";
                    m_userStatus.color = m_offlineColor;
                    m_userStatusDot.color = m_offlineColor;
                    m_userStatusBar.color = m_offlineColor;
                    break;
                case StatusType.InGame:
                    m_userStatus.text = "En Partie";
                    m_userStatus.color = m_inGameColor;
                    m_userStatusDot.color = m_inGameColor;
                    m_userStatusBar.color = m_inGameColor;
                    break;
                case StatusType.InLobby:
                    m_userStatus.text = "Dans un lobby";
                    m_userStatus.color = m_inGameColor;
                    m_userStatusDot.color = m_inGameColor;
                    m_userStatusBar.color = m_inGameColor;
                    break;
                default:
                    m_userStatus.text = "En Ligne";
                    m_userStatus.color = m_onlineColor;
                    m_userStatusDot.color = m_onlineColor;
                    m_userStatusBar.color = m_onlineColor;
                    break;
            }
        }
    }
}
