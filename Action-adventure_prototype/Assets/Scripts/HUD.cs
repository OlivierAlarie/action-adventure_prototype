using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _healthText;

    void Update()
    {
        _healthText.SetText("Health : "+GameMaster.Instance.Player.Health);
    }
}
