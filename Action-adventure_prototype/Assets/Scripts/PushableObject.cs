using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableObject : MonoBehaviour
{
    [SerializeField] private float _speed;
    private Vector3 _destination;
    private bool _beingPushed;
    private bool _wouldCollide;
    private RaycastHit _hit;
    private Vector3 _lastDirection;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_beingPushed)
        {
            if(Vector3.Distance(transform.position, _destination) < Mathf.Epsilon)
            {
                transform.position = _destination;
                _beingPushed = false;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, _destination, _speed * Time.deltaTime);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_wouldCollide)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, _lastDirection*2);
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position + _lastDirection * 2, transform.localScale * 0.97f);
            Debug.Log(_hit.collider.gameObject.name);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, _hit.point - transform.position);
        }
        
    }

    private bool CheckDirection(Vector3 direction)
    {
        
        _wouldCollide = Physics.BoxCast(transform.position, transform.localScale / 2.05f , direction, out _hit, Quaternion.identity, 2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        if (_wouldCollide)
        {
            _lastDirection = direction;
            return false;
        }
        else
        {
            return true;
        }
    }

    public void Push(Vector3 direction)
    {
        if (!_beingPushed)
        {
            if (CheckDirection(direction))
            {
                _destination = transform.position + (direction*2);
                _beingPushed = true;
            }
        }
    }

}
