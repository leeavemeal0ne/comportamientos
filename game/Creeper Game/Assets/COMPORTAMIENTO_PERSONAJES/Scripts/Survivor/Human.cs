using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;

public class Human : MonoBehaviour {
    protected float health = StandardConstants.MAX_HEALTH;
    public int ammo = StandardConstants.MAX_AMMO;

    protected Animator anim;

    public bool startIdle = false;

    // Use this for initialization
    protected void Start () {
        anim = GetComponent<Animator>();
        if (startIdle)
        {
            //anim.SetTrigger("Idle");
            //anim.SetBool("Idle", true);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TakeDamage(float damage)
    {
        health -= damage;
        if(health<= 0)
        {
            print(name + ": He muerto");
            StartCoroutine("Die");
        }
        else
        {
            StartCoroutine("GetShot");
        }
    }

    public bool IsDead()
    {
        return health <= 0;
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

    IEnumerator GetShot()
    {
        yield return new WaitForSeconds(1.0f);
        anim.SetTrigger("GetShot");
    }

    IEnumerator Die()
    {
        yield return new WaitForSeconds(1.0f);
        anim.SetTrigger("Die");
        Destroy(GetComponent<Human>());
    }


}
