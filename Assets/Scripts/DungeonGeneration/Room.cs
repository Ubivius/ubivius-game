using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public int width;
    public int height;
    public int X;
    public int Y;

    // Start is called before the first frame update
    void Start()
    {
        //if(RoomController.instance == null)
        //{
        //    Debug.Log("You pressed play inthe wrong scene");
        //    return;
        //}
    }

    public Vector2 GetRoomCenter()
    {
        return new Vector2(X * width, Y * height);
    }
}
