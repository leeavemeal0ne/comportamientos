using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using comportamiento_personajes;

public class Pain_Death_Test : MonoBehaviour {

    private Basic_zombie_behaviour zb;
    private bool called;

    private void Start()
    {
        called = false;
        zb = GameObject.FindObjectOfType<Basic_zombie_behaviour>();
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0) && !called)
        {
            called = true;
            Debug.Log("TAKE DAMAGE LLAMO");
            zb.TakeDamage(25);
        }
        if (Input.GetMouseButtonUp(0))
        {
            called = false;
        }
    }
}
