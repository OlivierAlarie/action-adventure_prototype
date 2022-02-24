using UnityEngine;

public class Catcher : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.transform.parent = transform.parent;
            Debug.Log("Parenting to Platform");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.transform.parent = null;
            Debug.Log("Deparenting");
        }
    }
}