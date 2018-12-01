using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;

namespace comportamiento_personajes
{
    public class zombie_attack : MonoBehaviour
    {
        //private string TAG_ATTACK;

        public Transform parent_transform;
        //para entrar dentro del if
        private bool available;
        //path por el que va a moverse el zombie
        public AnimationClip animation_attack;
        private SphereCollider spCol;

        public zombie_sight zs;

        private void Start()
        {
            spCol = GetComponentInParent<SphereCollider>();

            //Si está disponible para ejecutar la animación
            available = true;
            //transform del padre
            parent_transform = GetComponentInParent<Transform>();
            Debug.Log("TAG = " + zs.TAG  /*+ " ZB = " + zb_attack.currentState.ToString()*/);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (zs.zb.feeding || !zs.Attack(other) || zs.getEnemySightCount() <= 0)
            {
                return;
            }

            //Debug.Log("Entro en attack Collider, is Attacking = " + zb_attack.getisAttacking());
            if (zs.Attack(other))
            {
                Debug.Log("ONTRIGGER ENTER ENTRO SEGUNDO IF DEBERÍA ATACAR");
                zs.zb.setisAttacking(true);
                zs.zb.setCurrentState(AIStates.Attack);

                if (available)
                {
                    zs.zb.setAgentParameters(0,0);
                    zs.zb.resetPersecutionPoint();
                    zs.zb.setAnimatorTriggerParameters("Attack_trigger");
                    StartCoroutine(animationFinish());
                }
                
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (zs.zb.feeding || !zs.Attack(other) || zs.getEnemySightCount() <= 0)
            {
                //Debug.Log("SALGO");
                return;
            }

            float distance = Vector3.Distance(parent_transform.position, zs.target.transform.position);
            Debug.Log("Distancia a OBJETIVO: " + distance);

            if(distance < 1f)
            {
                if (zs.zb.animator.GetFloat("Speed") > 0.1f)
                {
                    //Debug.Log("VELOCIDAD A 0");
                    zs.zb.setAnimatorParameters("Speed", 0);
                    zs.zb.setAgentParameters(0, 0);
                }
            }
            else
            {
                zs.zb.setAgentParameters(zs.zb.getAgentSpeed(), zs.zb.getAgentRotationSpeed());
                zs.zb.setAnimatorParameters("Speed", 1);
            }

            if (zs.Attack(other) && available)
            {
                //zs.zb.transform.rotation = Quaternion.Lerp(zs.zb.transform.rotation, zs.target.transform.rotation, 2 * Time.deltaTime);
                //zs.zb.transform.LookAt(zs.target.transform);
                zs.zb.setAnimatorTriggerParameters("Attack_trigger");
                StartCoroutine(animationFinish());                         
            }

            if (zs.target != null && zs.Attack(other))
            {
                zs.zb.transform.LookAt(zs.target.transform);
                //zs.zb.transform.rotation = Quaternion.Lerp(zs.zb.transform.rotation, zs.target.transform.rotation, 5 * Time.deltaTime);
            }
        }

        //Si salimos del collider attack volvemos al estado alerta
        private void OnTriggerExit(Collider other)
        {
            if (zs.zb.feeding) return;

            if (zs.Attack(other))
            {
                Debug.Log("OnTriggerExit de zombie_attack");
                zs.zb.backToAlert();
            }               
        }

        //No hacemos nada mientras la animación no ha terminado así ahorramos llamar demasiado de seguido a las animaciones
        private IEnumerator animationFinish()
        {
            available = false;
            //Debug.Log("Duracion de la animacion ATACAR = " + animation_attack.length);
            yield return new WaitForSeconds(animation_attack.length);
            available = true;
            StopCoroutine(animationFinish());
        }
    }
}
