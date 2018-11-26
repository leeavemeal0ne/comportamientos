using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using comportamiento_personajes;

public class Pain_Death_Test : MonoBehaviour {

    private Basic_zombie_behaviour zb;

    private void Start()
    {
        zb = GameObject.FindObjectOfType<Basic_zombie_behaviour>();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("TAKE DAMAGE LLAMO");
            zb.TakeDamage(25);
        }
    }
}
