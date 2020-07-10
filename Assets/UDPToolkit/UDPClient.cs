using UnityEngine;
using System.Collections;

/// <summary>
/// MOCK CLIENT (interface is valid)
/// </summary>
public class UDPClient : MonoBehaviour
{
    [SerializeField] private UDPServer m_server;

    private UDPToolkit.ConnectionData m_connectionData;

    private void Awake()
    {
        m_connectionData = new UDPToolkit.ConnectionData();
    }

    private void Start()
    {
        // connect to server
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Send(uint Data)
    {
        m_server.Receive(new UDPToolkit.Packet(Data, m_connectionData.LocalSequence++, m_connectionData.RemoteSequence, 0), this);
    }

    public void Receive(UDPToolkit.Packet packet)
    {
        if(packet.Sequence > m_connectionData.RemoteSequence)
            m_connectionData.RemoteSequence = packet.Sequence;
    }
}
