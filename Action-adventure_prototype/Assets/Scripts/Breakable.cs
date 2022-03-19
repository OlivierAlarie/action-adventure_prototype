using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    [SerializeField] private GameObject _objectToDestroy;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Destroy");
        if(other.tag == "PlayerWeapon")
        {
            Destroy(_objectToDestroy);
            gameObject.SetActive(false);
        }
    }
}
