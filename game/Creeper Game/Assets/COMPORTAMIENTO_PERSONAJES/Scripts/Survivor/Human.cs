using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;

public class Human : MonoBehaviour {
    protected float health = StandardConstants.MAX_HEALTH;
    protected int ammo = StandardConstants.MAX_AMMO;


    protected Animator anim;

    // Use this for initialization
    protected void Start () {
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    public bool isWounded()
    {
        bool ret = false;
        if (health < StandardConstants.MAX_HEALTH)
        {
            ret = true;
        }
        return ret;
    }

    public void GiveAmmo(Human giver, int quantity)
    {
        int giverAmmo = giver.getAmmo();
        if (giverAmmo >= quantity)
        {
            print(giver.name + " gives "+ quantity +" ammo to " + gameObject.name);
            ammo += quantity;
        }
        else
        {
            print(giver.name + " gives " + giverAmmo + " ammo to " + gameObject.name);
            ammo += giverAmmo;
        }
        giver.removeAmmo(quantity);
    }

    public int getAmmo()
    {
        return ammo;
    }

    public void removeAmmo(int quantity)
    {
        if(quantity<= ammo)
        {
            ammo -= quantity;
        }
        else
        {
            ammo = 0;
        }

        print("Me queda la siguiente ammo: " + ammo);
    }


}
