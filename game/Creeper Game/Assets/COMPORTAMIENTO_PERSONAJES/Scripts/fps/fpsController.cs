using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;
using comportamiento_personajes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class fpsController : Zombie {

    private string DEFAULT_TEXT_TOTALMUNICION = "Munición Total: ";
    private string DEFAULT_TEXT_CARGADOR = "Cargador: ";
    private string DEFAULT_LIFE = "Health: ";

    private float speed = 2;
    private int Health = 100;
    private bool playerIsDead = false;

    private float minimumRotationVertical = 45;
    private float maximumRotationVertical = -45;

    public float speedH = 2.0f;
    public float speedV = 2.0f;
    private float yaw = 180;
    private float pitch = 0.0f;

    private float rotationX = 0;
    private float rotationY = 0;
    private float cantidadMovimiento = 0.05f;

    public Transform gun;
    public Transform weapon;
    public ParticleSystem shotParticles;
    public Transform shotPosition;
    public Camera fpsCamera;
    private Animator anim;
    public TextMeshProUGUI txtPro;

    //munición
    public int municionTotal = 250;
    private int municionCargador = 20;
    private int DEFAULT_BALAS_EN_CARGADOR = 20;

	// Use this for initialization
	void Start () {
        gameObject.tag = Tags.PLAYER;
        transform.rotation.Set(0, 180, 0, 1);
        gun.transform.rotation.Set(0, 180, 0,gun.transform.rotation.w);
        fpsCamera = GetComponentInChildren<Camera>();

        anim = GetComponent<Animator>();
        updateText();
	}
	
	// Update is called once per frame
	void Update () {
        //recargamos la escena cuando estemos muertos
        if (playerIsDead && Input.GetKeyDown(KeyCode.S))
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        if (playerIsDead) return;

        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");

        if(pitch > minimumRotationVertical)
        {
            pitch = minimumRotationVertical;
        }
        if(pitch < maximumRotationVertical)
        {
            pitch = maximumRotationVertical;
        }

        gun.transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        transform.eulerAngles = new Vector3(0, gun.transform.eulerAngles.y, 0);

        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 2;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 2;
        
        Vector3 movement = new Vector3(x, 0, z);
        //movement = Vector3.ClampMagnitude(movement, speed);

        transform.Translate(movement);

        Debug.DrawRay(gun.position, gun.forward, Color.red);

        if (Input.GetMouseButtonDown(0))
        {
            shot();
        }
        if (Input.GetKey(KeyCode.R))
        {
            reload();
        }
        
    }

    private void shot()
    {
        bool wall = false;
        if(municionCargador >= 0)
        {
            municionCargador--;
            shotParticles.transform.position = shotPosition.position;
            shotParticles.transform.rotation = shotPosition.rotation;
            shotParticles.Play();
            RaycastHit[] hits;
            hits = Physics.RaycastAll(weapon.position, weapon.forward, 100.0F);
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider.gameObject.tag == Tags.WALL)
                {
                    wall = true;
                }
                
                if (!wall && hit.collider.gameObject.tag == Tags.NORMAL_ZOMBIE)
                {
                    Debug.Log("NOMBRE DE A QUIEN DOY = " + hit.collider.gameObject.name);
                    hit.collider.gameObject.GetComponent<Zombie>().TakeDamage(10);
                }

                updateText();

            }
            
            
        }
       

        
    }
        

    private void reload()
    {
          int balas = DEFAULT_BALAS_EN_CARGADOR - municionCargador;
          if(balas <= municionTotal)
          {
             municionCargador += balas;
             municionTotal -= balas;
            anim.SetTrigger("Reload_trigger");
          }
          else if(balas > municionTotal && municionTotal > 0)
          {
             balas = municionTotal;
             municionCargador -= balas;
             municionTotal -= balas;
            anim.SetTrigger("Reload_trigger");
          }
        updateText();
    }

    private void updateText()
    {
        txtPro.text = DEFAULT_TEXT_TOTALMUNICION + municionTotal + "\n"
                      + DEFAULT_TEXT_CARGADOR + municionCargador+"\n" +
                      DEFAULT_LIFE + Health;
    }

    public override void startToEat()
    {
        throw new System.NotImplementedException();
    }

    public override void TakeDamage(int dmg)
    {
        if (!playerIsDead)
        {
            Debug.Log("Me hacen pupita--------");
            Health -= dmg;
            if (Health <= 0)
            {
                playerIsDead = true;
                gameObject.tag = Tags.DEATH_ZOMBIE;
                transform.position = new Vector3(-22.31f, -2.28f, 45.401f);
                GetComponent<Rigidbody>().useGravity = false;
            }
            updateText();
        }       
    }

    public override bool getIsDead()
    {
        throw new System.NotImplementedException();
    }
}
