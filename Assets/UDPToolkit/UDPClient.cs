using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// MOCK CLIENT (interface is valid)
/// </summary>
public class UDPClient : MonoBehaviour
{
    [SerializeField] private bool m_running = true;
    [SerializeField] private float m_mockWaitTime = 0.005f;
    private Queue<UDPToolkit.Packet> m_receivedPackets;
    [SerializeField] private UDPServer m_server;

    private UDPToolkit.ConnectionData m_connectionData;

    private void Awake()
    {
        m_receivedPackets = new Queue<UDPToolkit.Packet>();
        m_connectionData = new UDPToolkit.ConnectionData();
        StartCoroutine(ListenCoroutine());
    }

    private void Start()
    {
        // connect to server
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Send(uint data)
    {
        m_server.Receive(new UDPToolkit.Packet(data, 
            m_connectionData.LocalSequence++,
            m_connectionData.RemoteSequence,
            m_connectionData.GetACKBitfield()), this);
    }

    public void Receive(UDPToolkit.Packet packet)
    {
        m_receivedPackets.Enqueue(packet);
    }

    private void OnReceive(UDPToolkit.Packet packet)
    {
        m_connectionData.Receive(packet);
        Debug.Log("Received in client " + packet.Data.ToString());
    }

    private IEnumerator ListenCoroutine()
    {
        while (m_running)
        {
            yield return new WaitForSecondsRealtime(m_mockWaitTime);

            if (m_receivedPackets.Count > 0)
            {
                UDPToolkit.Packet packet = m_receivedPackets.Dequeue();
                if (UDPToolkit.HasValidProtocolID(packet))
                {
                    OnReceive(packet);
                }
            }
        }
    }
}
