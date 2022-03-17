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
    public ArenaManager Arena;
    public Transform _pointTogo;
    public Transform _pointTocome;
    [SerializeField]private float _attackDistance = 3f;
    public bool FollowingEnemy = false;
    private bool _toA = true;
    private bool _toB = false;
    private float _hurtTimer = 1f;
    private enum AiState
    {
        Wandering,
        Following,
        Attacking,
        Hurting
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
        float distToPlayer;
        if (Arena != null && Arena.Player != null) 
        {
            distToPlayer = Vector3.Distance(transform.position, Arena.Player.transform.position);
        }
        else
        {
            distToPlayer = float.PositiveInfinity;
        }
        

        if (_currentState == AiState.Wandering)
        {
            _animator.Play("Walk");
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
           
            else if (distToPlayer < _boundry)
            {
                _currentState = AiState.Following;
                _toA = false;
                _toB = false;
            } 
        }

        else if (_currentState == AiState.Following)
        {
            _animator.Play("Walk");
            FollowingEnemy = true;
            if(Arena.Player != null) { SetDestination(Arena.Player.transform.position); }
            _toA = false;
            _toB = false;
            
            if (distToPlayer <= _attackDistance)
            {
                _currentState = AiState.Attacking;
            }
            if (distToPlayer >= _boundry)
            {
                _currentState = AiState.Wandering;
                FollowingEnemy = false;
                _toA = true;
                _toB = false;
            }
        }
        else if(_currentState == AiState.Attacking)
        {
            _animator.Play("Attack");
            _agent.stoppingDistance = _attackDistance;
            if (distToPlayer > _attackDistance && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                _currentState = AiState.Following;
            }
            if (Arena.Player != null) { FaceTarget(Arena.Player.transform.position); }
        }
        else if(_currentState == AiState.Hurting)
        {
            _hurtTimer -= Time.deltaTime;

            _animator.Play("Idle");
            if (_hurtTimer < 0f)
            {
                _currentState = AiState.Attacking;
                _hurtTimer = 1f;
            }
            Arena.OnEnemyDestroyed(this);
        }

   
    }

    void SetDestination(Vector3 point)
    {
        _agent.SetDestination(point);
        _agent.stoppingDistance = 0;
        FaceTarget(point);
    }

    void FaceTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "PlayerWeapon")
        {
            _currentState = AiState.Hurting;
        }
    }

}

