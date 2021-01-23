using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;

namespace ubv
{
    namespace client
    {
        namespace logic
        {
            abstract public class ClientSyncState
            {
                protected udp.client.UDPClient m_client;
                protected readonly object m_lock = new object();

                public ClientSyncState(udp.client.UDPClient client)
                {
                    m_client = client;
                }

                public abstract ClientSyncState Update();
                public abstract ClientSyncState FixedUpdate();
            }

            public class ClientSyncInit : ClientSyncState, udp.client.IPacketReceiver
            {
                //tranfer to play
                private readonly string m_physicsScene;
                private readonly InputController m_inputController;
                private int m_playerID;

#if NETWORK_SIMULATE
                private bool m_playWithoutServer;
#endif // NETWORK_SIMULATE

                public ClientSyncInit(udp.client.UDPClient client, 
                    string physicsScene, 
                    InputController inputController
#if NETWORK_SIMULATE
                    , ClientSync parent)
#endif // NETWORK_SIMULATE
                    : base(client)
                {
                    m_physicsScene = physicsScene;
                    m_inputController = inputController;
                    m_playerID = -1;

                    m_playWithoutServer = false;

                    m_client.RegisterReceiver(this);

#if NETWORK_SIMULATE
                    parent.ConnectButtonEvent.AddListener(SendConnectionRequestToServer);
                    parent.PlayWithoutServerButtonEvent.AddListener(() => { m_playWithoutServer = true; });
#endif // NETWORK_SIMULATE
                }

                public override ClientSyncState FixedUpdate()
                {
                    return this;
                }

                public void SendConnectionRequestToServer()
                {
                    m_client.Send(new IdentificationMessage().GetBytes()); // sends a ping to the server
                }

                public void ReceivePacket(UDPToolkit.Packet packet)
                {
                    // receive auth message and set player id
                    // UDP FOR NOW, TCP LATER (with auth)

                    IdentificationMessage auth = udp.Serializable.FromBytes<IdentificationMessage>(packet.Data);
                    if (auth != null)
                    {
                        m_playerID = (int)auth.PlayerID.Value;
                        Debug.Log("Received connection confirmation, player ID is " + m_playerID);
                    }
                }

                public override ClientSyncState Update()
                {
                    if(m_playerID > -1
#if NETWORK_SIMULATE
                    || m_playWithoutServer
#endif // NETWORK_SIMULATE
                    )
                    {
                        return new ClientSyncPlay(m_client, m_playerID, m_physicsScene, m_inputController);
                    }

                    return this;
                }
            }

            /// <summary>
            /// Represents the state of the server during the game
            /// </summary>
            public class ClientSyncPlay : ClientSyncState, udp.client.IPacketReceiver
            {
                private readonly int m_playerID;

                private uint m_remoteTick;
                private uint m_localTick;

                private const ushort CLIENT_STATE_BUFFER_SIZE = 64;

                private ClientState[] m_clientStateBuffer;
                private common.data.InputFrame[] m_inputBuffer;
                private common.data.InputFrame m_lastInput;
                private ClientState m_lastServerState;

                private PhysicsScene2D m_clientPhysics;

                private readonly InputController m_inputController;

#if NETWORK_SIMULATE
                private readonly float m_packetLossChance = 0.15f;
#endif // NETWORK_SIMULATE

                public ClientSyncPlay(udp.client.UDPClient client, int playerID, string physicsScene, InputController inputController) : base(client)
                {
                    m_playerID = playerID;
                    m_localTick = 0;
                    m_clientStateBuffer = new ClientState[CLIENT_STATE_BUFFER_SIZE];
                    m_inputBuffer = new common.data.InputFrame[CLIENT_STATE_BUFFER_SIZE];

                    m_clientPhysics = SceneManager.GetSceneByName(physicsScene).GetPhysicsScene2D();
                    m_lastServerState = null;

                    m_inputController = inputController;

                    if (m_playerID > -1)
                    {
                        m_client.RegisterReceiver(this);
                    }

                    for (ushort i = 0; i < CLIENT_STATE_BUFFER_SIZE; i++)
                    {
                        common.data.PlayerState player = new common.data.PlayerState();
                        m_clientStateBuffer[i] = new ClientState();

                        if (m_playerID > -1)
                        {
                            m_clientStateBuffer[i].SetPlayerID((uint)m_playerID);
                        }

                        m_clientStateBuffer[i].AddPlayer(player);
                        m_inputBuffer[i] = new common.data.InputFrame();
                    }
                }

                public override ClientSyncState FixedUpdate()
                {
                    uint bufferIndex = m_localTick % CLIENT_STATE_BUFFER_SIZE;

                    UpdateInput(bufferIndex);

                    UpdateClientState(bufferIndex);

                    ++m_localTick;

                    ClientCorrection();

                    return this;
                }

                public override ClientSyncState Update()
                {
                    m_lastInput = m_inputController.CurrentFrame();

                    return this;
                }

                public void ReceivePacket(UDPToolkit.Packet packet)
                {
                    // TODO remove tick from ClientSTate and add it to custom server state packet?
                    // client doesnt need its own client state ticks
                    lock (m_lock)
                    {
                        ClientState state = udp.Serializable.FromBytes<ClientState>(packet.Data);
                        if (state != null)
                        {
                            state = udp.Serializable.FromBytes<ClientState>(packet.Data);
                        }

                        if (state != null)
                        {
                            m_lastServerState = state;
#if DEBUG_LOG
                            Debug.Log("Received server state tick " + state.Tick);
#endif //DEBUG_LOG
                            m_remoteTick = state.Tick;
                        }
                    }
                }

                private void UpdateInput(uint bufferIndex)
                {
                    if (m_lastInput != null)
                    {
                        m_inputBuffer[bufferIndex].Movement.Set(m_lastInput.Movement.Value);
                        m_inputBuffer[bufferIndex].Sprinting.Set(m_lastInput.Sprinting.Value);
                    }
                    else
                    {
                        m_inputBuffer[bufferIndex].SetToNeutral();
                    }

                    m_inputBuffer[bufferIndex].Tick.Set(m_localTick);

                    m_lastInput = null;

                    if (m_playerID > -1)
                    {
                        // TODO: Cap max input queue size
                        // (under the hood, send multiple packets?)
                        List<common.data.InputFrame> frames = new List<common.data.InputFrame>();
                        for (uint tick = m_remoteTick; tick <= m_localTick; tick++)
                        {
                            frames.Add(m_inputBuffer[tick % CLIENT_STATE_BUFFER_SIZE]);
                        }

                        common.data.InputMessage inputMessage = new common.data.InputMessage();

                        inputMessage.PlayerID.Set((uint)m_playerID);
                        inputMessage.StartTick.Set(m_remoteTick);
                        inputMessage.InputFrames.Set(frames);

#if NETWORK_SIMULATE
                        if (Random.Range(0f, 1f) > m_packetLossChance)
                        {
                            m_client.Send(inputMessage.GetBytes());
                        }
                        else
                        {
                            Debug.Log("SIMULATING PACKET LOSS");
                        }
#else
                        m_udpClient.Send(inputMessage.ToBytes());
#endif //NETWORK_SIMULATE       
                    }
                }

                private void UpdateClientState(uint bufferIndex)
                {
                    // set current client state to last one then updating it
                    m_clientStateBuffer[bufferIndex].StoreCurrentStateAndStep(
                        m_inputBuffer[bufferIndex],
                        Time.fixedDeltaTime,
                        ref m_clientPhysics);
                }

                private void ClientCorrection()
                {
                    if (m_playerID < 0)
                    {
                        return;
                    }

                    // receive a state from server
                    // check what tick it corresponds to
                    // rewind client state to the tick
                    // replay up to local tick by stepping every tick
                    lock (m_lock)
                    {
                        if (m_lastServerState != null)
                        {
                            List<IClientStateUpdater> updaters = ClientState.UpdatersNeedingCorrection(m_lastServerState);
                            for (int i = 0; i < updaters.Count; i++)
                            {
                                uint rewindTicks = m_remoteTick;

                                // reset world state to last server-sent state
                                updaters[i].UpdateFromState(m_lastServerState);

                                while (rewindTicks < m_localTick)
                                {
                                    uint rewindIndex = rewindTicks++ % CLIENT_STATE_BUFFER_SIZE;

                                    updaters[i].SetStateAndStep(
                                        ref m_clientStateBuffer[rewindIndex],
                                        m_inputBuffer[rewindIndex],
                                        Time.fixedDeltaTime);
                                    m_clientPhysics.Simulate(Time.fixedDeltaTime);
                                }
                            }

                            m_lastServerState = null;
                        }
                    }
                }
            }
        }
    }
}
