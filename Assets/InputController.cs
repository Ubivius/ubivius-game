﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [Header("Movement parameters")]
    [SerializeField] private float m_velocity;
    [SerializeField] private float m_acceleration;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // collect input (calls to Input.GetXYZ.(...))
        // compute if needed (ex: trigo with cursor position)
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        // apply input to body according to rules of movement (of your choosing)
    }
}
