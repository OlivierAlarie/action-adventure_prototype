using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationReceiver : MonoBehaviour
{
    [SerializeField] private BasicPlayerController _player;
    // Start is called before the first frame update
    void Start()
    {
        _player = GetComponentInParent<BasicPlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Clear()
    {
        _player.ClearHorizontalMotion();
    }
}
