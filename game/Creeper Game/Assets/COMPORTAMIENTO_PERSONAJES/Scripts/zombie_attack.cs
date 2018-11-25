using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace comportamiento_personajes
{
    public class zombie_attack : zombie_sight
    {
        //private string TAG_ATTACK;

        private Transform parent_transform;
        //para entrar dentro del if
        private bool available;
        //path por el que va a moverse el zombie
        public AnimationClip animation_attack;

        private void Awake()
        {
            //Si está disponible para ejecutar la animación
            available = true;
            //transform del padre
            parent_transform = GetComponentInParent<Transform>();
            Debug.Log("TAG = " + TAG  /*+ " ZB = " + zb_attack.currentState.ToString()*/);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (zb.feeding)
            {
                return;
            }

            //Debug.Log("Entro en attack Collider, is Attacking = " + zb_attack.getisAttacking());
            if (other.tag.Equals(TAG))
            {
                zb.setisAttacking(true);
                zb.setCurrentState(AIStateType.Attack);

                if (available)
                {
                    zb.setAgentParameters(0, 0);
                    zb.resetPersecutionPoint();
                    zb.setAnimatorTriggerParameters("Attack_trigger");
                    StartCoroutine(animationFinish());
                }
                
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (zb.feeding)
            {
                return;
            }

            if (other.tag.Equals("Player") && available)
            {
                if(zb.animator.GetFloat("Speed") > 0.1f)
                {
                    Debug.Log("VELOCIDAD A 0");
                    zb.setAnimatorParameters("Speed", 0);
                    
                }
                zb.setAnimatorTriggerParameters("Attack_trigger");
                StartCoroutine(animationFinish());               
            }
            if (other.tag.Equals(TAG))
            {
                //OJO NOSE SI FUNCIONA MUY BIEN ESTO -----------REVISAR----------------
                //Rotamos para golpear hacia el superviviente aunque nose si esto lo hace muy bien
                parent_transform.Rotate(other.transform.eulerAngles);
            }
            
            //parent_transform.LookAt(other.transform.position, Vector3.up);
        }

        //Si salimos del collider attack volvemos al estado alerta
        private void OnTriggerExit(Collider other)
        {
            if (zb.feeding) return;

            if (other.tag.Equals(TAG))
            {
                Debug.Log("OnTriggerExit de zombie_attack");
                zb.backToAlert();
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
