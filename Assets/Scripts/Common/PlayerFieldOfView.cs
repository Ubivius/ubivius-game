using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv
{
    namespace common
    {
        namespace logic
        {

            public class PlayerFieldOfView : MonoBehaviour
            {
                static public void Execute(ref Rigidbody2D rigidbody, FieldOfViewGameObjectUpdater fieldOfView, common.data.InputFrame input, float deltaTime)
                {
                    Vector2 movement;
                    movement.x = Input.GetAxisRaw("Horizontal");
                    movement.y = Input.GetAxisRaw("Vertical");

                    Vector2 mousePos = fieldOfView.cam.ScreenToWorldPoint(Input.mousePosition);

                    Vector3 lookDir = mousePos - rigidbody.position;
                    float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
                    rigidbody.rotation = angle;

                    //field of view
                    fieldOfView.SetAimDirection(lookDir);
                    fieldOfView.SetOrigin(rigidbody.position);
                }

            }
        }
    }
}
