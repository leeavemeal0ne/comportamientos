using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;

namespace comportamiento_personajes
{
    public class ZombieEliteVision : MonoBehaviour
    {

        List<string> tagsList;
        [HideInInspector]
        public List<GameObject> zombiesList;

        // Use this for initialization
        void Start()
        {
            tagsList = new List<string>(10);
            tagsList.Add(Tags.NORMAL_ZOMBIE);
            tagsList.Add(Tags.FAST_ZOMBIE);

            zombiesList = new List<GameObject>(100);
        }

        #region OnTriggerStay-Exit
        private void OnTriggerEnter(Collider other)
        {
            if (tagsList.Contains(other.tag))
            {
                if (!zombiesList.Contains(other.gameObject))
                {
                    zombiesList.Add(other.gameObject);
                }
                //Debug.Log("DEBERÍA DESHABILITAR LOS SCRIPTS");           
                other.GetComponentInParent<Zombie>().notifyPeace();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (tagsList.Contains(other.tag))
            {
                if (!zombiesList.Contains(other.gameObject))
                {
                    zombiesList.Add(other.gameObject);
                }
                //Debug.Log("DEBERÍA DESHABILITAR LOS SCRIPTS");           
                other.GetComponentInParent<Zombie>().notifyPeace();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (tagsList.Contains(other.tag))
            {
                if (zombiesList.Contains(other.gameObject))
                {
                    zombiesList.Remove(other.gameObject);
                }
                Debug.Log("DEBERÍA HABILITAR LOS SCRIPTS");
                other.GetComponentInParent<Zombie>().leavePeace();
            }
        }
        #endregion
    }
}
