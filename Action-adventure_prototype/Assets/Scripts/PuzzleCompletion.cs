using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleCompletion : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PushableObject")
        {
            //SpawnKey
            other.gameObject.GetComponent<PushableObject>().IsLocked = true;
        }
    }
}
