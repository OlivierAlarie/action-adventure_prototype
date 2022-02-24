using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    [SerializeField] private float _addTimeValue = 2f;
    public SwitchWorld _switchWorld;
   

    private void Awake() {
        if (_switchWorld == null) {
            _switchWorld = GameObject.FindWithTag("Player").GetComponent<SwitchWorld>();
        } else {
            Debug.Log("orb is null");
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            _switchWorld._timeLeft += _addTimeValue;
            this.gameObject.SetActive(false);
            Destroy(this.gameObject,2f);
        }
    }
}
