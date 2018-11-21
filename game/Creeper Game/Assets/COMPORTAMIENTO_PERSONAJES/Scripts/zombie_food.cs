using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace comportamiento_personajes
{
    public class zombie_food : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            string name = LayerMask.LayerToName(other.gameObject.layer);
            if (name.Equals("Zombie"))
            {
                other.GetComponent<zombie_behaviour>().startToEat();
            }
        }
    }
}
