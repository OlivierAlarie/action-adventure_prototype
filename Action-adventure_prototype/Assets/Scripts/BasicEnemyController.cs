using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyController : MonoBehaviour
{
    [SerializeField] private Animator _enemyAnimator;
    private EnemyStates _currentState;

    private float _timer = 3f;
    
    private enum EnemyStates
    {
        Idle,
        Run,
        Roll,
        Fall,
        Hurt
    }
    // Start is called before the first frame update
    void Start()
    {

        _currentState = EnemyStates.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        RunStates();
        Move();
    }

    private bool IsAnimatorPlaying()
    {
        return _enemyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
    }

    private bool IsAnimatorMatchingState(string stateName)
    {
        return _enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    void RunStates()
    {
        switch (_currentState)
        {
            case EnemyStates.Idle:
                Idle();
                break;
            case EnemyStates.Run:
                Run();
                break;
            case EnemyStates.Roll:
                Roll();
                break;
            case EnemyStates.Fall:
                Fall();
                break;
            case EnemyStates.Hurt:
                Hurt();
                break;
        }
    }

    private void StartIdle()
    {
        _currentState = EnemyStates.Idle;
        _enemyAnimator.Play("Idle");
    }
    private void Idle()
    {
        StopIdle();
    }
    private void StopIdle()
    {
        if(_timer == 0)
        {
            StartRun();
        }
    }

    private void StartRun()
    {
        _currentState = EnemyStates.Run;
        _enemyAnimator.Play("Run");
    }
    private void Run()
    {
        StopRun();
    }
    private void StopRun()
    {
    }

    private void StartRoll()
    {
        _currentState = EnemyStates.Roll;
    }
    private void Roll()
    {
        StopRoll();
    }
    private void StopRoll()
    {

    }

    private void StartFall()
    {
        _currentState = EnemyStates.Fall;
    }
    private void Fall()
    {
        StopFall();
    }
    private void StopFall()
    {

    }

    private void StartHurt()
    {
        _currentState = EnemyStates.Hurt;
        _enemyAnimator.Play("Hurt");
    }
    private void Hurt()
    {
        StopHurt();
    }
    private void StopHurt()
    {
        if (IsAnimatorMatchingState("Hurt") && !IsAnimatorPlaying())
        {
            StartIdle();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlayerWeapon")
        {
            StartHurt();
        }
    }

    private void Move()
    {
        
    }
}
