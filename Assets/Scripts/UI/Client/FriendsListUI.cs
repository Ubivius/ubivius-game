using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ubv.client.logic;
using ubv.microservices;
using UnityEngine.Events;

namespace ubv.ui.client
{
    [RequireComponent(typeof(RectTransform))]
    public class FriendsListUI : MonoBehaviour
    {
        private SocialServicesController m_socialServices;

        [Header("UI components")]
        [SerializeField] private GameObject m_friendsList;
        [SerializeField] private GameObject m_friendsListBox;
        [SerializeField] private GameObject m_invitesListBox;
        [SerializeField] private FriendsListHeader m_inviteHeader;
        [SerializeField] private FriendsListHeader m_friendHeader;
        [SerializeField] private InviteUI m_invitePrefab;
        [SerializeField] private FriendUI m_friendPrefab;
        [SerializeField] private TextMeshProUGUI m_username;
        [SerializeField] private TextMeshProUGUI m_userStatus;
        [SerializeField] private Image m_userStatusDot;
        [SerializeField] private Button m_showButton;
        [SerializeField] private Button m_hideButton;

        [Header("Colors")]
        [SerializeField] private Color m_onlineColor;
        [SerializeField] private Color m_offlineColor;
        [SerializeField] private Color m_inGameColor;

        private Dictionary<string, InviteUI> m_invitesPrefabList;
        private Dictionary<string, FriendUI> m_friendsPrefabList;

        private Queue<RelationInfo> m_newInviteQueue;
        private Queue<RelationInfo> m_newFriendQueue;
        private Queue<RelationInfo> m_updateFriendQueue;

        private UserInfo m_activeUser;

        public bool FriendsListIsHidden { get; set; }

        private void Awake()
        {
            m_newInviteQueue = new Queue<RelationInfo>();
            m_newFriendQueue = new Queue<RelationInfo>();
            m_updateFriendQueue = new Queue<RelationInfo>();
            m_socialServices = ClientSyncState.SocialServices;
            m_invitesPrefabList = new Dictionary<string, InviteUI>();
            m_friendsPrefabList = new Dictionary<string, FriendUI>();
        }

        private void Update()
        {
            while (m_newInviteQueue.Count > 0)
            {
                RelationInfo invite = m_newInviteQueue.Dequeue();
                InviteUI invitePrefab = Instantiate(m_invitePrefab, m_invitesListBox.transform);
                m_invitesPrefabList.Add(invite.RelationID, invitePrefab);
                invitePrefab.SetInvite(invite);
            }
            while (m_newFriendQueue.Count > 0)
            {
                RelationInfo friend = m_newFriendQueue.Dequeue();
                FriendUI friendPrefab = Instantiate(m_friendPrefab, m_friendsListBox.transform);
                m_friendsPrefabList.Add(friend.RelationID, friendPrefab);
                friendPrefab.SetFriend(friend);
            }
            while (m_updateFriendQueue.Count > 0)
            {
                RelationInfo friend = m_updateFriendQueue.Dequeue();
                m_friendsPrefabList[friend.RelationID].SetFriend(friend);
            }
        }

        void Start() {
            FriendsListIsHidden = true;
            ToggleDisplayFriendsList();
            m_activeUser = m_socialServices.CurrentUser;
            m_username.text = m_activeUser.UserName;
            if(m_activeUser.Status != StatusType.Online)
            {
                UpdateStatus(StatusType.Online);
            }
            SetStatus(m_activeUser.Status);
            m_socialServices.OnNewInvite += OnNewInvite;
            m_socialServices.OnNewFriend += OnNewFriend;
            m_socialServices.UpdateFriend += UpdateFriend;
            m_socialServices.OnDeleteInvite += OnDeleteInvite;
        }

        private void OnNewInvite(RelationInfo invite)
        {
            m_newInviteQueue.Enqueue(invite);
        }

        private void OnNewFriend(RelationInfo friend)
        {
            m_newFriendQueue.Enqueue(friend);
        }

        private void UpdateFriend(RelationInfo friend)
        {
            m_updateFriendQueue.Enqueue(friend);
        }

        private void OnDeleteInvite(string relationID)
        {
            m_invitesPrefabList.Remove(relationID);
        }

        public void ToggleDisplayFriendsList() {
            FriendsListIsHidden = !FriendsListIsHidden;

            m_showButton.gameObject.SetActive(FriendsListIsHidden);
            m_hideButton.gameObject.SetActive(!FriendsListIsHidden);
        }

        public void Hide() {
            m_friendsList.SetActive(false);
            FriendsListIsHidden = true;

            m_hideButton.gameObject.SetActive(false);
            m_showButton.gameObject.SetActive(true);
        }

        public void Show() {
            m_friendsList.SetActive(true);
            FriendsListIsHidden = false;

            m_hideButton.gameObject.SetActive(true);
            m_showButton.gameObject.SetActive(false);
        }

        public void UpdateStatus(StatusType status)
        {
            m_activeUser.Status = status;
            m_socialServices.UpdateUserStatus(status);
        }

        public void ToggleStatus()
        {
            if (m_activeUser.Status == StatusType.Online)
            {
                SetStatus(StatusType.Offline);
                UpdateStatus(StatusType.Offline);
            }
            else
            {
                SetStatus(StatusType.Online);
                UpdateStatus(StatusType.Online);
            }
        }

        public void SetStatus(StatusType status)
        {
            switch (status)
            {
                case StatusType.Offline:
                    m_userStatus.text = "Hors Ligne";
                    m_userStatus.color = m_offlineColor;
                    m_userStatusDot.color = m_offlineColor;
                    break;
                case StatusType.InGame:
                    m_userStatus.text = "En Partie";
                    m_userStatus.color = m_inGameColor;
                    m_userStatusDot.color = m_inGameColor;
                    break;
                case StatusType.InLobby:
                    m_userStatus.text = "Dans un lobby";
                    m_userStatus.color = m_inGameColor;
                    m_userStatusDot.color = m_inGameColor;
                    break;
                default:
                    m_userStatus.text = "En Ligne";
                    m_userStatus.color = m_onlineColor;
                    m_userStatusDot.color = m_onlineColor;
                    break;
            }
        }
    }
}
