using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFollow : MonoBehaviour
{

    [SerializeField]
    private float _speed = 1f;
    [SerializeField]
    private float _range = 1;
    [SerializeField]
    private float _boundry = 5;
    [SerializeField]
    private int _minX = -20;
    [SerializeField]
    private int _maxX = 20;
    [SerializeField]
    private int _minZ = -20;
    [SerializeField]
    private int _maxZ = 20;
    private AiState _currentState;
    public Transform Player;
    private Vector3 _pointTogo;
    public bool FollowingEnemy = false;
    private enum AiState
    {
        Wandering,
        Following
    }
    void Start()
    {
        SetDestination();
        _currentState = AiState.Wandering;
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentState == AiState.Wandering)
        {
            transform.position = Vector2.MoveTowards(transform.position, _pointTogo, _speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, _pointTogo) < _range)
            {
                SetDestination();
            }
            else if (Vector3.Distance(transform.position, Player.transform.position) < _boundry)
            {
                _currentState = AiState.Following;
            }

        }

        else if (_currentState == AiState.Following)
        {
            transform.position = Vector2.MoveTowards(transform.position, Player.transform.position, _speed * Time.deltaTime);
            FollowingEnemy = true;
            if (Vector2.Distance(transform.position, Player.transform.position) >= _boundry)
            {
                _currentState = AiState.Wandering;
                FollowingEnemy = false;
            }
        }

        Debug.Log("enemy's current State: " + _currentState);
        Debug.Log("enemy wants to go " + _pointTogo);
    }

    void SetDestination()
    {
        float xPos = Random.Range(_minX, _maxX);
        float zPos = Random.Range(_minZ, _maxZ);
        float yPos = transform.position.y;
        _pointTogo = new Vector3(xPos, yPos, zPos);
    }
}

