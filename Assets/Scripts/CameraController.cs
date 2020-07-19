using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Manages the movement of the main camera
    [SerializeField] private Transform m_playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        m_playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        // if we wanted to follow the object directly, we'd do something like
        // transform.position = m_objectToFollow.position; 
        // this does not account for the z difference, so it's not gonna work but you get the idea
    }

    private void LateUpdate()
    {
        Vector3 temp = transform.position;

        temp.x = m_playerTransform.position.x;
        temp.y = m_playerTransform.position.y;

        transform.position = temp;
    }
}
