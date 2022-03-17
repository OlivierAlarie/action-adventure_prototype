using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public bool IsOpen;
    private Vector3 _targetHeight;
    // Start is called before the first frame update
    void Start()
    {
        _targetHeight = new Vector3(transform.position.x, transform.position.y - transform.localScale.y , transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOpen) return;

        transform.position = Vector3.MoveTowards(transform.position, _targetHeight, Time.deltaTime);
    }
}
