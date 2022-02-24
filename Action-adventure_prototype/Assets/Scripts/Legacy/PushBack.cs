using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBack : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private float _explosionForce = 5f;
    [SerializeField] private float _explosionRadius = 2f;
    public GameObject wraithPos;
    public Rigidbody rb;

    private void Start() {
        rb.detectCollisions = false;
        rb.isKinematic = true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        if (hit.gameObject.tag == "Wraith") {
            _characterController.detectCollisions = false;
            rb.detectCollisions = true;
            rb.isKinematic = false;
            Debug.Log("is colliding");
            rb.AddExplosionForce(_explosionForce, wraithPos.transform.position, _explosionRadius, 0.0f, ForceMode.Impulse);
        }
    }
    
}
