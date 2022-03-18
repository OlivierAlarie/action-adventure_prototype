using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public bool IsOpen;
    [SerializeField] private float _speed;
    private Vector3 _closedPos;
    private Vector3 _openPos;
    // Start is called before the first frame update
    void Start()
    {
        _closedPos = transform.position;
        _openPos = new Vector3(transform.position.x, transform.position.y - 5f , transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPos = IsOpen ? _openPos : _closedPos;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * _speed);
    }
}
