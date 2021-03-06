using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTrigger : MonoBehaviour
{
    public Transform Spawnpoint;
    public GameObject key;
    public GameObject player;
    bool isTriggered;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter()
    {
        if (!isTriggered)
        {
            Instantiate(key, Spawnpoint.position, Spawnpoint.rotation);
            Debug.Log("You solved the puzzle, go open the door");
            isTriggered = true;
        }   
    }
}
