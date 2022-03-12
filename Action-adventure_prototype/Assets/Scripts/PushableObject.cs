using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableObject : MonoBehaviour
{
    [SerializeField] private float _speed;
    private Vector3 _destination;
    private bool _beingPushed;
    

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
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {

        }
    }

    private bool CheckDirection(Vector3 direction)
    {
        if (Physics.BoxCast(transform.position, transform.localScale / 2, direction, Quaternion.identity,2f))
        {
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
