using UnityEngine;
using UnityEditor;

public interface IReceiver
{
    void Receive(UDPToolkit.Packet packet);
}