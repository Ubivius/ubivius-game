using UnityEngine;
using UnityEditor;

public interface ISender
{
    void Send(uint data, ISenderReceiver target);
}