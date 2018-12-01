using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fpsController : MonoBehaviour {

    private float speed = 10.0f;
    private float cantidadMovimiento = 0.05f;
    public Transform gun;
    public ParticleSystem shotParticles;
    public Transform shotPosition;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //rotacion con el ratón
        transform.Rotate(new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * Time.deltaTime * speed);

        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 2;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 2;

        //transform.Rotate(0, x, 0);
        transform.Translate(x, 0, z);

        Debug.DrawRay(gun.position, gun.forward, Color.red);

        if (Input.GetMouseButtonDown(0))
        {
            shot();
        }
    }

    private void shot()
    {
        shotParticles.transform.position = shotPosition.position;
        shotParticles.transform.rotation = shotPosition.rotation;
        shotParticles.Play();
    }
}
