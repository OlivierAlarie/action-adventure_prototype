using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wraith : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] public BasicPlayerController player;
    [SerializeField] public GameObject centerPoint;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float minDistance = 10f;
    [SerializeField] public float knockBackDuration = 1f;
    [SerializeField] public float knockBackForce = 25f;

    private Vector3 _direction;

    private void Start()
    {

        if (player == null)
        {
            player = FindObjectOfType<BasicPlayerController>();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 playerPos = player.transform.position + Vector3.up;
        float distance = Vector3.Distance(playerPos, centerPoint.transform.position);


        //moves towards player
        if (distance <= minDistance) {
            if (!GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().Play();
            }
            transform.LookAt(playerPos);
            _direction = playerPos - rb.position;
            _direction = _direction.normalized;
            rb.velocity = _direction * speed;
        } 
        //returns to origin point
        if (distance > minDistance) {
            _direction = centerPoint.transform.position - rb.position;
            _direction = _direction.normalized;
            float distanceWraithToCenter = Vector3.Distance(transform.position, centerPoint.transform.position);
            if (distanceWraithToCenter <= 0.1f) {
                rb.velocity = Vector3.zero;
                transform.rotation = Quaternion.Euler(-90,0,0);
            }
            else
            {
                rb.velocity = _direction * speed;
                transform.LookAt(centerPoint.transform.position);
            }
        }
    }
    private void OnCollisionStay(Collision collision)
    {

        if (collision.gameObject.name == "PlayerCharacter")
        {
            player.KnockBack(-1 * collision.GetContact(0).normal, knockBackDuration, knockBackForce);
        }
    }

}
