using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestOpenSequence : MonoBehaviour
{
    public KeyTracker keyTracker;
    public Animator chestAnimator;
    public float numberOfKeysNeeded = 4f;
    public GameObject timerBar;
    public GameObject player;
    public AudioSource chestsound;
    public TextMeshProUGUI textdefin;
    public GameObject lepanneau;
    private float timer = -1f;

    [Header("Cameras")]
    public GameObject mainCam;
    public GameObject newParent;

    private void Start()
    {
        textdefin.enabled = false;
        lepanneau.SetActive(false);
    }
    private void Update()
    {
        if (timer > -1)
        {
        if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                textdefin.enabled = true;
                lepanneau.SetActive(true);
            }
            
        }
    }


    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player" && keyTracker._numberOfKeys == numberOfKeysNeeded && keyTracker._hasSpecialKey) {
            chestAnimator.SetBool("hasSpecialKey", true);
            timerBar.SetActive(false);
            chestsound.Play();
            mainCam.transform.parent = newParent.transform;
            mainCam.transform.localPosition = Vector3.zero;
            mainCam.transform.localRotation = Quaternion.identity;
            player.SetActive(false);
            if (keyTracker.numberOfRubis == 100)
            {
                textdefin.text = "Congratulation on finishing the game 100%\n\nThere are no more secrets for you to find.\n\nExcept MAYBE the little green guy\n\nOn Behalf of Etienne, Charles-Antoine, Olivier and Benoit,\n Thank you for playing our game!";
            }
            if (keyTracker.numberOfRubis < 100)
            {
                textdefin.text = "Congratulation on finishing the game\n\nand finding the hidden key!!\n\n Now if you wish to complete the game 100%,\n you can replay and try to find the " + (100-keyTracker.numberOfRubis) + " Gems that are still missing\n\nOn Behalf of Etienne, Charles-Antoine, Olivier and Benoit,\n Thank you for playing our game!";
            }
            timer = 4f;
        }
        if (other.gameObject.tag == "Player" && keyTracker._numberOfKeys == numberOfKeysNeeded && !keyTracker._hasSpecialKey)
        {
            chestAnimator.SetBool("hasFullKey", true);
            timerBar.SetActive(false);
            chestsound.Play();
            mainCam.transform.parent = newParent.transform;
            mainCam.transform.localPosition = Vector3.zero;
            mainCam.transform.localRotation = Quaternion.identity;
            textdefin.text = "Congratulation on finishing the game\n\n Now if you wish to complete the game 100%, and open the little chest,\n you can replay and try to to find the " + (100 - keyTracker.numberOfRubis) + " Gems that are still missing\nAs well as the hidden key\n\nOn Behalf of Etienne, Charles-Antoine, Olivier and Benoit,\n Thank you for playing our game!";
            player.SetActive(false);
            timer = 4f;
        }
    }
}
