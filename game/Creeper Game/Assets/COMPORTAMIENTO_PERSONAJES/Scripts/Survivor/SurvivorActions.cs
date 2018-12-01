using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;

public class SurvivorActions : MonoBehaviour {

    private SurvivorBehaviour survivor;
	// Use this for initialization
	void Start () {
        survivor = GetComponentInParent<SurvivorBehaviour>();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == survivor.getTarget() && other.tag != "Untagged")
        {
            print("He alcanzado a mi objetivo " + other.name);
            survivor.setCanDoAction(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == survivor.getTarget() && other.tag != "Untagged")
        {
            print("Se ha escapado el pinche puto" + other.name);
            //survivor.setCanDoAction(false);
        }
    }
}
