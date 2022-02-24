using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbRespawn : MonoBehaviour
{
    public GameObject orbPrefab;
    public float cooldownTimer = 5f;
    

    void Update()
    {
        if (transform.childCount < 1) {
            CountDown();
            if (cooldownTimer < 0) {
                Instantiate(orbPrefab, transform);
                cooldownTimer = 5f;
            }
        }
    }

    void CountDown() {
        cooldownTimer -= Time.deltaTime;
    }
}
