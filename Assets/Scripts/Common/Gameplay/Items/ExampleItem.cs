using UnityEngine;
using System.Collections;

namespace ubv.common.gameplay
{
    public class ExampleItem : PlayerItem
    {
        protected override void ItemActivation()
        {
            Debug.Log("Dummy item activated");
        }
    }
}
