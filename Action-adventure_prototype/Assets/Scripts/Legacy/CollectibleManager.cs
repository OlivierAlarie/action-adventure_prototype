using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectibleManager : MonoBehaviour
{
    public TextMeshProUGUI rubisCount;
    public TextMeshProUGUI keyShardCount;
    public GameObject specialKeyShardImage;
    public KeyTracker keyTracker;
    

    

    void Update()
    {
        rubisCount.text = keyTracker.numberOfRubis.ToString("0");
        keyShardCount.text = keyTracker._numberOfKeys.ToString("0");
        if (keyTracker._hasSpecialKey) {
            specialKeyShardImage.SetActive(true);
        } else {
            specialKeyShardImage.SetActive(false);
        }
    }
}
