using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;
using comportamiento_personajes;
using UnityEngine.AI;
using UnityEngine.UI;

public class Human : Zombie {
    protected float health = StandardConstants.MAX_HEALTH;
    public int ammo = StandardConstants.MAX_AMMO;

    protected Animator anim;

    public bool startIdle = false;

    public Text healthText;
    public Text ammoText;

    // Use this for initialization
    protected void Start () {
        gameObject.tag = Tags.SURVIVOR;

        anim = GetComponent<Animator>();
        if (startIdle)
        {
            //anim.SetTrigger("Idle");
            anim.SetBool("Idle", true);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void startToEat()
    {
        throw new System.NotImplementedException();
    }

    public override void TakeDamage(int damage)
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

        healthText.text = "Health: " + health + "/" + StandardConstants.MAX_HEALTH;
    }

    public override void notifyDead()
    {
        //throw new System.NotImplementedException();
    }

    public override bool getIsDead()
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
        setAmmoText();
        giver.setAmmoText();
    }

    public void setAmmoText()
    {
        if (ammoText != null)
        {
            ammoText.text = "Ammo: " + ammo;
        }
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
        //yield return new WaitForSeconds(1.0f);
        anim.SetTrigger("Die");
        //GetComponent<Rigidbody>().useGravity = false;
        Collider[] c = GetComponentsInChildren<Collider>();
        Destroy(GetComponent<NavMeshAgent>());
        //Destroy(this);
        Destroy(GetComponent<Rigidbody>());
        foreach(Collider col in c)
        {
            Destroy(col);
        }

        yield return null;
    }


}
