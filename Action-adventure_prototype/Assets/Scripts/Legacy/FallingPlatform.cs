using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] public float moveSpeed = -2f;
    [SerializeField] public float timeBeforeFalling = 3f;
    [SerializeField] private bool hasCollided = false;

    // Update is called once per frame
    void Update()
    {
        if (hasCollided) {
            transform.position += new Vector3(transform.position.x, transform.position.y * moveSpeed * Time.deltaTime, transform.position.z);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Player") {
            Debug.Log("player is on the platform");
            hasCollided = true;
        }
    }
}
