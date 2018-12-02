using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;

namespace comportamiento_personajes
{
    public class zombie_sight : MonoBehaviour
    {
        public string TAG = "Player";

        private List<string> TagList;

        //public NavMeshAgent agent;
        [HideInInspector]
        public Basic_zombie_behaviour zb;
        public float fieldOfViewAngle = 110f;
        protected bool playerInSight;
        private Vector3 personalLastSighting;
        private Vector3? previousSighting;
        private List<GameObject> players;

        public GameObject target;
        protected string target_name;
        private List<GameObject> playerInCollider;
        private List<GameObject> EnemySight;

        private float distance;
        private float minDistance;

        private float waitSeconds;

        private SphereCollider col;

        #region getters/setters
        public List<GameObject> getPlayers() { return players; }
        public Basic_zombie_behaviour GetBasic_Zombie_Behaviour() { return zb; }
        public string getTAG() { return TAG; }
        public bool Attack(Collider other)
        {
            bool temp = other.gameObject.name.Equals(target_name);
            return temp;
        }
        public int getEnemySightCount() { return EnemySight.Count; }
        public zombie_sight GetInstance() { return this; }
        #endregion

        // Use this for initialization
        void Start()
        {
            TagList = new List<string> { Tags.SURVIVOR, Tags.PLAYER };

            //no objetivo
            target = null;
            //Distancia al objetivo
            distance = 100;
            minDistance = 3;           

            //Buscamos todos los gameObjects con tag Player
            players = new List<GameObject>(20);
            List<GameObject> temp = new List<GameObject>(10);
            foreach (string t in TagList)
            {
                temp = GameObject.FindGameObjectsWithTag(t).ToList();
                players.AddRange(temp);
            }
            players = GameObject.FindGameObjectsWithTag(TAG).ToList();
            Debug.Log("Numero de players en lista: " + players.Count);
            col = GetComponent<SphereCollider>();
            //player o superviviente es visible
            playerInSight = false;
            //tiempo que le damos por si vuelve a entrar en el collider el superviviente
            waitSeconds = 0.5f;
            zb = GetComponentInParent<Basic_zombie_behaviour>();
            playerInCollider = new List<GameObject>(20);
            EnemySight = new List<GameObject>(20);

            target_name = "";
        }

        private void OnTriggerEnter(Collider other)
        {
            //Si la lista no contiene al objeto que entra sale
            if (!players.Contains(other.gameObject) || zb.feeding || zb.isAttacking)
            {
                return;
            }
            else
            {
                //cambiamos el estado 
                if (canSeeEnemy(other))
                {
                    if (zb.currentState == AIStates.Patrol)
                    {
                        zb.setCurrentState(AIStates.Alerted);
                    }
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {

            //Si esta comiendo no hace nada o si está atacando
            if (!players.Contains(other.gameObject) || zb.feeding || zb.isAttacking)
            {
                return;
            }

            //primero vemos si puedo ver al enemigo y después elegimos el objetivo si hay
            canSeeEnemy(other);
            chooseTarget();
        }

        private void OnTriggerExit(Collider other)
        {
            if (zb.feeding) return;

            //cuando sale de nuestra área de influencia borramos del array y actualizamos nuestro estado
            if (EnemySight.Contains(other.gameObject))
            {
                Debug.Log("Borro el enemigo a la salida");
                EnemySight.Remove(other.gameObject);
            }
            chooseTarget();



        }

        /// <summary>
        /// Elige el objetivo, tenemos una lista de enemigos a la vista, si no está a la vista no hacemos nada y volvemos a
        /// modo patrulla
        /// </summary>
        private void chooseTarget()
        {
            //Si no hay enemigos a la vista volvemos a patrol
            if (EnemySight.Count <= 0)
            {
                if (zb.currentState == AIStates.Alerted)
                {
                    Debug.Log("BACK TO PATROL");
                    distance = 100;
                    target = null;
                    zb.backToPatrol();
                }
            }
            //si solo hay un objetivo le elegimos
            else if (EnemySight.Count == 1)
            {
                //Debug.Log("ENEMY SIGHT = 1, tag = " + EnemySight[0].name);
                target = EnemySight[0];
                distance = Vector3.Distance(target.transform.position, this.transform.position);
            }
            //calculamos cuál está mas cerca
            else
            {
                //Debug.Log("ENEMY SIGHT = " + EnemySight.Count);
                foreach (GameObject g in EnemySight)
                {
                    //Si la distancia al objetivo es menor a 3 no calculamos nada nos quedamos con el target que tengamos
                    if (distance < 3)
                    {
                        Debug.Log("Distance < 2");
                        if (g.Equals(target))
                        {
                            //Debug.Log("Me quedo con mi target = " + g.name);
                            target = g;
                            distance = Vector3.Distance(target.transform.position, this.transform.position);
                        }
                    }
                    else
                    {
                        //Elegimos el de menor distancia
                        float dis = Vector3.Distance(g.transform.position, this.transform.position);
                        if (dis < distance)
                        {
                            //Debug.Log("Cambio target = " + g.name);
                            target = g;
                            distance = dis;
                        }
                    }
                }
            }
            //actualizamos posiciones y destino del navmeshAgent
            if (target != null)
            {
                target_name = target.name;
                //Debug.Log("Target seleccionado = " + target.name);
                //zb.transform.rotation = Quaternion.Lerp(zb.transform.rotation, target.transform.rotation, 5 * Time.deltaTime);
                zb.agent.SetDestination(target.transform.position);
            }
        }

        /// <summary>
        /// Calcula si vemos al personaje con un angulo prefijado si está dentro de ese ángulo el superviviente
        /// y no hay nada entre el y el zombie le perseguimos
        /// sino no hacemos nada
        /// </summary>
        /// <param name="other"></param>
        private bool canSeeEnemy(Collider other)
        {
            bool canSee = false;

            if (players.Contains(other.gameObject))
            {
                Vector3 direction = other.transform.position - zb.transform.position;
                float angle = Vector3.Angle(direction, transform.forward);

                Debug.DrawRay(zb.transform.position, transform.forward, Color.green);

                if (angle < fieldOfViewAngle * 0.5f)
                {
                    RaycastHit hit;

                    Debug.DrawRay(transform.position, direction, Color.black);
                    bool colliderHit = Physics.Raycast(transform.position, direction.normalized, out hit, col.radius);

                    if (colliderHit && hit.collider.gameObject.tag.Equals(TAG))
                    {
                        canSee = true;
                        if (!EnemySight.Contains(other.gameObject))
                        {
                            //Debug.Log("AÑADO ENEMIGO GameObject ENEMYSIGHT");
                            EnemySight.Add(other.gameObject);
                        }
                    }
                    else
                    {
                        //Debug.Log("Borrado GameObject ENEMYSIGHT");
                        EnemySight.Remove(other.gameObject);
                    }
                    direction = Quaternion.Euler(0, -40, 0) * direction;
                    Debug.DrawRay(transform.position, direction, Color.blue);
                    direction = Quaternion.Euler(0, 80, 0) * direction;
                    Debug.DrawRay(transform.position, direction, Color.blue);
                    Debug.DrawLine(transform.position, hit.point, Color.red);
                }
            }

            return canSee;
        }

        private IEnumerator wait()
        {
            yield return new WaitForSeconds(waitSeconds);
            playerInSight = false;
            zb.backToPatrol();

            //currentState = AIStateType.Patrol;
            StopAllCoroutines();
        }
    }
}