using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;
using comportamiento_personajes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Throwable : MonoBehaviour {

    private bool updatePos;
    private Rigidbody r;
    private Vector3 force;
    private float speed;
    private Vector3 rotationVel;
    

    // Use this for initialization
    void Start() {
        updatePos = false;
        r = GetComponent<Rigidbody>();
        speed = 25;
        rotationVel = new Vector3(1, 0, 0);
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (updatePos) {
            Debug.Log(force);
            r.useGravity = true;
            transform.position += force * speed * Time.deltaTime;
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //si da a un zombie o superviviente le tiene que quitar vida
        if (other.gameObject.tag == Tags.PLAYER)
        {
            other.gameObject.GetComponent<fpsController>().TakeDamage(10);
            Destroy(this);
        }
        else if (other.gameObject.tag == Tags.SURVIVOR || other.gameObject.tag == Tags.VISUAL_SURVIVOR)
        {
            other.gameObject.GetComponent<SurvivorBehaviour>().TakeDamage(10);
            Destroy(this);
        }
        
    }

    public void updatePhysics() {
        updatePos = true;
    }

    
    public void setForce(Vector3 target)
    {

        /*
        Vector3 dir = target - transform.position; // get Target Direction
        float height = dir.y; // get height difference
        dir.y = 0; // retain only the horizontal difference
        float dist = dir.magnitude; // get horizontal direction
        float a = angle * Mathf.Deg2Rad; // Convert angle to radians
        dir.y = dist * Mathf.Tan(a); // set dir to the elevation angle.
        dist += height / Mathf.Tan(a); // Correction for small height differences

        // Calculate the velocity magnitude
        float velocity = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));
        force = velocity * dir.normalized; // Return a normalized vector.
        */

        force = (target - transform.position).normalized;
    }

}
