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
    [SerializeField]
    private int _health = 3;
    [SerializeField]
    private Transform _pointTogo;
    private Vector3 _pointTocome;
    private AiState _currentState;
    public ArenaManager Arena;

    
    [SerializeField]private float _attackDistance = 3f;
    public bool FollowingEnemy = false;
    private bool _toA = true;
    private bool _toB = false;
    private float _hurtTimer;
    private float _totalHurtDuration = 1f;
    private Vector3 _lastHurtDirection;
    private CharacterController _controller;

    private enum AiState
    {
        Idleing,
        Wandering,
        Following,
        Attacking,
        Hurting,
        Dying
    }
    void Start()
    {  
        if(_agent == null)
        {
            _agent = GetComponent<NavMeshAgent>();
        }
        _currentState = AiState.Idleing;
        _pointTocome = transform.position;
        SetDestination(_pointTogo.position);
        _controller = GetComponent<CharacterController>();
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
        
        if (_currentState == AiState.Idleing)
        {
            _animator.Play("Idle");
            _agent.isStopped = true;
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                _agent.isStopped = false;
                _currentState = AiState.Wandering;
            }
            if (distToPlayer < _boundry)
            {
                _agent.isStopped = false;
                _currentState = AiState.Following;
            }
        }
        else if (_currentState == AiState.Wandering)
        {
            _animator.Play("Walk");
            if (_agent.remainingDistance <= _range && _toA == true && _toB == false)
            {
                _currentState = AiState.Idleing;
                SetDestination(_pointTocome);
                _toA = false;
                _toB = true;
            }
            else if (_agent.remainingDistance <= _range && _toA == false && _toB == true)
            {
                _currentState = AiState.Idleing;
                SetDestination(_pointTogo.position);
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
            _agent.isStopped = true;
            //Change to Hurt, Find HurtAnimation
            if(_hurtTimer > _totalHurtDuration - 0.25f)
            {
                _controller.Move(_lastHurtDirection * 10 * Time.deltaTime);
            }
            if (_hurtTimer < 0f)
            {
                _currentState = AiState.Wandering;
                _agent.isStopped = false;
            }
        }
        else if (_currentState == AiState.Dying)
        {
            _animator.Play("Death");
            _agent.isStopped = true;
            _controller.enabled = false;

            if(Arena != null && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                Arena.OnEnemyDestroyed(this);
            }
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
            _lastHurtDirection = transform.position - other.GetComponentInParent<BasicPlayerController>().transform.position;
            _lastHurtDirection.y = 0;
            _lastHurtDirection.Normalize();

            _health--;
            if(_health <= 0)
            {
                _currentState = AiState.Dying;
            }
            else
            {
                _currentState = AiState.Hurting;
                _hurtTimer = _totalHurtDuration;
                _animator.Play("Hurt",-1,0f);
            }
            
        }
    }

}

