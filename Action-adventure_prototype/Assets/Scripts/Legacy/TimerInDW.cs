using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerInDW : MonoBehaviour
{
    private Image _timerBar;
    public float maxTime = 10f;
    private float _timeLeft;

    private void Start() {
        _timerBar = GetComponent<Image>();
        _timeLeft = maxTime;
    }

    void Update()
    {
        if (_timeLeft > 0) {
            _timeLeft -= Time.deltaTime;
            _timerBar.fillAmount = _timeLeft / maxTime;
        }
    }
}
