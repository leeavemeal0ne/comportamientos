using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;

public class SurvivorDetection : MonoBehaviour
{

    public SurvivorBehaviour survivor;
    // Use this for initialization
    void Start()
    {

        //survivor = GetComponentInParent<SurvivorBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerStay(Collider collision)
    {
        if (survivor.canDetect && !survivor.detectedHumans.Contains(collision.gameObject))
        {
            if (survivor.zombieTags.Contains(collision.gameObject.tag) || survivor.survivorTags.Contains(collision.gameObject.tag))
            {
                print("Detected " + collision.gameObject.tag);
                Vector3 direction = collision.gameObject.transform.position - survivor.head.position;
                float angle = Vector3.Angle(direction, transform.forward);
                if (angle < 30.0f)
                {
                    RaycastHit hit;
                    RaycastHit[] allHits;
                    allHits = Physics.RaycastAll(survivor.head.position, direction, Vector3.Distance(collision.gameObject.transform.position, survivor.head.position));
                    Debug.DrawRay(survivor.head.position, transform.TransformDirection(Vector3.forward) * 1000, Color.blue);
                    bool wall = false;
                    for (int i = 0; i < allHits.Length; i++)
                    {
                        if (allHits[i].transform.tag == Tags.WALL)
                        {
                            wall = true;
                        }
                        if (!wall && (survivor.zombieTags.Contains(allHits[i].transform.tag) || survivor.survivorTags.Contains(allHits[i].transform.tag)))
                        {
                            hit = allHits[i];
                            print("Llego");
                            float d = Vector3.Distance(collision.gameObject.transform.position, survivor.transform.position);
                            if (d < survivor.distance)
                            {
                                survivor.distance = d;
                                survivor.actualTarget = hit.transform.gameObject;
                                //survivor.StopAllCoroutines();
                                print("Changing target " + hit.transform.name);
                                if (survivor.zombieTags.Contains(hit.transform.tag))
                                {
                                    //Hemos detectado un zombie
                                    survivor.DetectZombi();

                                }
                                else
                                {
                                    //Hemos detectado un humano
                                    survivor.DetectHuman(hit.transform.gameObject);
                                }

                            }
                            return;
                        }

                    }

                }
            }
        }
    }
}
