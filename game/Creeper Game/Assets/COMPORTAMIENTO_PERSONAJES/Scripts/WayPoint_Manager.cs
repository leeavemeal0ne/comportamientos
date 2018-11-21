using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace comportamiento_personajes
{
    public class WayPoint_Manager : MonoBehaviour
    {
        private static WayPoint_Manager _minstance = null;

        // List of Transform references
        public List<Transform> Waypoints = new List<Transform>();

        // List of Transform references
        public List<Transform> zombie_food = new List<Transform>();

        private void Start()
        {
            if(_minstance == null)
            {
                _minstance = this;
            }
        }

        //getters
        public static WayPoint_Manager getInstance()
        {
            return _minstance;
        }

        /// <summary>
        /// Devolvemos una ruta al azar así cada zombie y cada superviviente tendrán rutas al azar
        /// </summary>
        /// <returns></returns>
        public List<Transform> getWayPointsPath()
        {
            List<Transform> randomList = new List<Transform>(Waypoints.Count);
            List<Transform> tempList = new List<Transform>(Waypoints.Count);
            tempList.AddRange(Waypoints);

            int randomIndex = 0;
            for (int i =0; i < Waypoints.Count; ++i)
            {
                randomIndex = Random.Range(0, (tempList.Count-1)); //elegimos al azar
                randomList.Add(tempList[randomIndex]); //añadimos a la lista
                tempList.RemoveAt(randomIndex); //evitamos duplicar
            }

            string path = "";

            return randomList; //return the new random list
        }

        public Transform get_closest_Zombie_food(Transform zombie_pos)
        {
            int index = 0;
            float distance = 50000;
            for(int i = 0; i < zombie_food.Count; ++i)
            {
                float temp = Vector3.Distance(zombie_pos.position, zombie_food[i].position);
                if(temp < distance)
                {
                    distance = temp;
                    index = i;
                }

            }

            return zombie_food[index];
        }
    }
}
