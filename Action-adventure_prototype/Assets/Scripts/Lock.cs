using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock : MonoBehaviour
{
    public Animator LeftDoorAnimator;
    public Animator RightDoorAnimator;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(GameMaster.Instance.Player.NumberOfKeys > 0)
            {
                GameMaster.Instance.Player.NumberOfKeys--;
                LeftDoorAnimator.Play("DoorLeft");
                RightDoorAnimator.Play("DoorRight");
                Destroy(gameObject);
            }
        }
    }
}
