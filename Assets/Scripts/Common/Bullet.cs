using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.tag.Equals("Player"))
        {
            Destroy(gameObject);
        }
    }
}
