using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// MOCK UDP SERVER CLASS
/// </summary>
public class UDPServer : MonoBehaviour
{
    private struct PacketClientPair
    {
        public UDPToolkit.Packet Packet;
        public UDPClient Client;

        public PacketClientPair(UDPToolkit.Packet packet, UDPClient client)
        {
            Packet = packet;
            Client = client;
        }
    }

    [SerializeField] private bool m_running = true;
    [SerializeField] private float m_mockWaitTime = 0.005f;
    private Queue<PacketClientPair> m_receivedPackets;
    private Dictionary<UDPClient, ClientConnection> m_clientConnections;

    private class ClientConnection
    {
        public float LastConnectionTime;
        public UDPToolkit.ConnectionData ConnectionData;

        public ClientConnection()
        {
            ConnectionData = new UDPToolkit.ConnectionData();
        }
    }

    private void Awake()
    {
        m_receivedPackets = new Queue<PacketClientPair>();
        m_clientConnections = new Dictionary<UDPClient, ClientConnection>();
        // start listening
        Listen();
    }

    private void Update()
    {
        
    }

    private void Listen()
    {
        StartCoroutine(ListenCoroutine());
    }

    void Send(uint data, UDPClient target)
    {
        target.Receive(new UDPToolkit.Packet(data,
            m_clientConnections[target].ConnectionData.LocalSequence,
            m_clientConnections[target].ConnectionData.RemoteSequence, 
            m_clientConnections[target].ConnectionData.GetACKBitfield()));
    }

    private void OnReceive(UDPToolkit.Packet packet, UDPClient source)
    {
        Debug.Log("Received in server " + packet.Data.ToString());
        
        if(!m_clientConnections.ContainsKey(source))
        {
            m_clientConnections.Add(source, new ClientConnection());
        }
        m_clientConnections[source].LastConnectionTime = Time.time;

        m_clientConnections[source].ConnectionData.Receive(packet);

        source.Receive(new UDPToolkit.Packet(packet.Data,
            m_clientConnections[source].ConnectionData.LocalSequence++,
            m_clientConnections[source].ConnectionData.RemoteSequence,
            m_clientConnections[source].ConnectionData.GetACKBitfield()));
    }
    
    private IEnumerator ListenCoroutine()
    {
        while (m_running)
        {
            yield return new WaitForSecondsRealtime(m_mockWaitTime);

            if (m_receivedPackets.Count > 0)
            {
                PacketClientPair pair = m_receivedPackets.Dequeue();
                if (UDPToolkit.HasValidProtocolID(pair.Packet))
                {
                    OnReceive(pair.Packet, pair.Client);
                }
            }
        }
    }

    public void Receive(UDPToolkit.Packet packet, UDPClient source)
    {
        // random chance of not receiving it (0 for now)
        m_receivedPackets.Enqueue(new PacketClientPair(packet, source));
    }
}
