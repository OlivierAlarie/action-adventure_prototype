using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class escapingChestAnim : MonoBehaviour
{
    public Animator chestAnimator;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            chestAnimator.SetBool("isEscaping", true);
        }
    }
}
