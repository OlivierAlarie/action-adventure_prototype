using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCameraController : MonoBehaviour
{
    private Camera _cam;
    private bool _switched;

    // Start is called before the first frame update
    void Start()
    {
        if(_cam == null)
        {
            _cam = GetComponent<Camera>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchWorld()
    {
        _cam.clearFlags = _switched ? CameraClearFlags.Skybox : CameraClearFlags.Color;
        _switched = !_switched;
    }
}
