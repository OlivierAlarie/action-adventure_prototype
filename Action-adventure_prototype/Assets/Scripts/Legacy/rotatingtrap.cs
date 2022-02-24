using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotatingtrap : MonoBehaviour
{
    // Start is called before the first frame update
    public float RotationDeX = 90f;
    public float RotationDeY = 0f;
    public float RotationDeZ = 0f;
    

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(RotationDeX * Time.deltaTime, RotationDeY * Time.deltaTime, RotationDeZ * Time.deltaTime, Space.Self);
    }
}
