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
        print(other.gameObject.name);
        if (other.gameObject == survivor.getTarget())
        {
            print("He alcanzado a mi objetivo");
            survivor.setCanDoAction(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == survivor.getTarget())
        {
            print("Se ha escapado el pinche puto");
            survivor.setCanDoAction(false);
        }
    }
}
