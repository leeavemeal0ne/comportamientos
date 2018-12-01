using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace comportamiento_personajes
{
    public class ObstacleZombie : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        private void LateUpdate()
        {
            transform.Rotate(new Vector3(0, 1, 0) * Time.deltaTime * 20, Space.World);
            //transform.rotation.eulerAngles.Set(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + 10, transform.rotation.eulerAngles.z);
        }
    }
}
