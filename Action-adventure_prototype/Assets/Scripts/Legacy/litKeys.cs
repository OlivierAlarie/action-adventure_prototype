using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class litKeys : MonoBehaviour
{
    public KeyTracker keyTracker;
    public GameObject key1;
    public GameObject key2;
    public GameObject key3;
    public GameObject key4;
    public Material newMaterial;

    // Update is called once per frame
    void Update()
    {
        if (keyTracker._numberOfKeys == 1) {
            key1.GetComponent<MeshRenderer>().material = newMaterial;
        }
        if (keyTracker._numberOfKeys == 2) {
            key2.GetComponent<MeshRenderer>().material = newMaterial;
        }
        if (keyTracker._numberOfKeys == 3) {
            key3.GetComponent<MeshRenderer>().material = newMaterial;
        }
        if (keyTracker._numberOfKeys == 4) {
            key4.GetComponent<MeshRenderer>().material = newMaterial;
        }
    }
}
