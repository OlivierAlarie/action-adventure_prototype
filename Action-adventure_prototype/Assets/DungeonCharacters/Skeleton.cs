using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Skeleton: MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent _agent;
    [SerializeField]
    private Animator _animator;
    //range = distance between the skeleton and point 
    [SerializeField]
    private float _range = 5f;
    // boundry: checking if player is in the boundry
    [SerializeField]
    private float _boundry = 5f;
    private AiState _currentState;
    public Transform Player;
    public Transform _pointTogo;
    public Transform _pointTocome;
    public bool FollowingEnemy = false;
    private bool _toA = true;
    private bool _toB = false;
    private enum AiState
    {
        Wandering,
        Following
    }
    void Start()
    {  
        if(_agent == null)
        {
            _agent = GetComponent<NavMeshAgent>();
        }
        _currentState = AiState.Wandering;
        SetDestination(_pointTogo.transform.position);

    }

    void Update()
    {
        if (_currentState == AiState.Wandering)
        {
            _animator.SetBool("walking", true);
            _animator.SetBool("attacking", false);


            if (_agent.remainingDistance <= _range && _toA == true && _toB == false)
            {
                SetDestination(_pointTocome.position);
                _toA = false;
                _toB = true;
            }
            else if (_agent.remainingDistance <= _range && _toA == false && _toB == true)
            {
                
                SetDestination(_pointTogo.transform.position);
                _toA = true;
                _toB = false;
            }
           
            else if (Vector3.Distance(transform.position, Player.transform.position) < _boundry)
            {
                _currentState = AiState.Following;
                _toA = false;
                _toB = false;
            } 
        }

        else if (_currentState == AiState.Following)
        {
            FollowingEnemy = true;
            _animator.SetBool("attacking", true);
            _animator.SetBool("walking", false);
            SetDestination(Player.transform.position);
            _toA = false;
            _toB = false;
            if (Vector3.Distance(transform.position, Player.transform.position) >= _boundry)
            {
                _currentState = AiState.Wandering;
                FollowingEnemy = false;
                _toA = true;
                _toB = false;
            }
        }

   
    }

    void SetDestination(Vector3 point)
    {
        _agent.SetDestination(point);
        FaceTarget(point);
    }

    void FaceTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

}

