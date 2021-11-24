using TMPro;
using UnityEngine;
using ubv.microservices;
using ubv.client.logic;

namespace ubv.ui.client
{
    [RequireComponent(typeof(RectTransform))]
    public class InviteUI : MonoBehaviour
    {
        [Header("UI components")]
        [SerializeField] private TextMeshProUGUI m_username;
        private SocialServicesController m_socialServices;

        private RelationInfo m_invite;
        private bool readyToDestroy = false;

        private void Awake()
        {
            m_socialServices = ClientSyncState.SocialServices;
        }

        private void Update()
        {
            if (readyToDestroy)
            {
                Destroy(gameObject);
            }
        }

        public void SetInvite(RelationInfo invite)
        {
            m_invite = invite;
            m_username.text = invite.FriendUsername;
        }

        public void AcceptInvite()
        {
            m_socialServices.AcceptInvite(m_invite, DetroyInvite);
        }

        public void DeclineInvite()
        {
            m_socialServices.DeclineInvite(m_invite.RelationID, DetroyInvite);
        }

        public void BlockUser()
        {
            m_socialServices.BlockUser(m_invite, DetroyInvite);
        }

        private void DetroyInvite()
        {
           readyToDestroy = true;
        }
    }
}
