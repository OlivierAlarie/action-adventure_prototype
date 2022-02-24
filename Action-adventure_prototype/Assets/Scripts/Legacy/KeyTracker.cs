using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTracker : MonoBehaviour
{
    [SerializeField] public bool _hasSpecialKey = false;
    [SerializeField] public int _numberOfKeys = 0;
    [SerializeField] public int numberOfRubis = 0;


    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Key")
        {
            _numberOfKeys++;
            other.gameObject.GetComponent<AudioSource>().Play();
            //  otherobject = GetComponent<AudioSource>();
            //  otherobject.Play();
            other.enabled = false;
            other.gameObject.transform.GetChild(0).gameObject.SetActive(false);
            other.gameObject.transform.GetChild(1).gameObject.SetActive(false);
            Destroy(other.gameObject, 2f);

        }
        if (other.gameObject.tag == "SpecialKey")
        {
            other.gameObject.GetComponent<AudioSource>().Play();
            other.enabled = false;
            other.gameObject.transform.GetChild(0).gameObject.SetActive(false);
            _hasSpecialKey = true;
            Destroy(other.gameObject, 1f);
        }
        if (other.gameObject.tag == "Rubis")
        {
            numberOfRubis++;
            other.gameObject.GetComponent<AudioSource>().Play();
            other.enabled = false;
            other.gameObject.transform.GetChild(0).gameObject.SetActive(false);
            Destroy(other.gameObject, 1f);
        }
        if (other.gameObject.tag == "Orb")
        {
            if (other.transform.childCount > 0)
            other.gameObject.GetComponent<AudioSource>().Play();
        }
    }
}
