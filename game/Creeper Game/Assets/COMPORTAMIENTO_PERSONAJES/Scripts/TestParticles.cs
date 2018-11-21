using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestParticles : MonoBehaviour {

    public ParticleSystem ps;

	// Use this for initialization
	void Start () {
        /*ParticleSystem system = ps;
        system.transform.position = this.transform.position;
        system.transform.rotation = this.transform.rotation;
        var settings = system.main;
        settings.simulationSpace = ParticleSystemSimulationSpace.World;
        system.Emit(10);
        var emission = system.emission;
        emission.enabled = true;*/
    }

    private void LateUpdate()
    {
        Destroy(this, 4);
    }

    private void OnApplicationQuit()
    {
        gameObject.SetActive(false);
    }
}
