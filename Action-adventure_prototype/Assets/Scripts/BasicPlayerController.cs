using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicPlayerController : MonoBehaviour
{
    [Header("Camera")]
    public Camera MainCamera;
    public Transform CameraRoot;
    public float CameraSensitivityX = 360;
    public float CameraSensitivityY = 360;
    public float MaxDownwardAngle = 20;
    public float MaxUpwardAngle = -60;

    private float _targetRotationH = 0;
    private float _targetRotationV = 0;
    private float _maxCameraDistance;
    private Vector3 _originalCameraLocalPosition;

    [Header("Character")]
    public Animator _playerAnimator;
    public CharacterController _playerController;
    public float MaxSpeed = 5f; //Target speed for the character
    public float SpeedChangeFactor = 5f;//Acceleration/Decceleration
    public float RollSpeed = 10f;//Target speed during roll
    public float JumpSpeed = 25f;
    public float JumpHeight = 1f; //Target height for the character
    public float JumpLeniency = 0.15f; // Seconds of leniency where a jump attempt is registered before reaching the ground / after leaving the ground without jumping
    public float Gravity = -9.81f;
    public float MaxDistanceTargetLock;
    public Transform LastCheckpoint;

    [Header("Model")]
    public Transform RootGeometry;

    //Inputs
    private PlayerInput _playerInputs;
    private Vector2 _inputMove;
    private Vector2 _inputLook;
    private bool _inputJump;
    private bool _inputAttack;

    private Vector3 _moveDirection = Vector3.zero;
    private Vector3 _moveMotion = Vector3.zero;
    [SerializeField] private float _targetVelocityXZ = 0f;
    private float _targetVelocityY = 0f;
    private float _terminalVelocityY = -53f;

    private PlayerStates _currentState;

    private enum PlayerStates
    {
        Idle,
        Jump,
        Run,
        Roll,
        Fall,
        Attack,
        Hurt
    }

    private void Awake()
    {
        _playerInputs = new PlayerInput();

        _playerInputs.CharacterControls.Move.started += OnMoveInput;
        _playerInputs.CharacterControls.Move.performed += OnMoveInput;
        _playerInputs.CharacterControls.Move.canceled += OnMoveInput;

        _playerInputs.CharacterControls.Jump.started += OnJumpInput;
        _playerInputs.CharacterControls.Jump.canceled += OnJumpInput;

        _playerInputs.CharacterControls.Look.started += OnLookInput;
        _playerInputs.CharacterControls.Look.performed += OnLookInput;
        _playerInputs.CharacterControls.Look.canceled += OnLookInput;

        _playerInputs.CharacterControls.Attack.started += OnAttackInput;
        _playerInputs.CharacterControls.Attack.canceled += OnAttackInput;
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        _inputMove = context.ReadValue<Vector2>();
    }

    private void OnLookInput(InputAction.CallbackContext context)
    {
        _inputLook = context.ReadValue<Vector2>();
    }

    private void OnJumpInput(InputAction.CallbackContext context)
    {
        _inputJump = context.ReadValueAsButton();
    }

    private void OnAttackInput(InputAction.CallbackContext context)
    {
        _inputAttack = context.ReadValueAsButton();
    }

    private void OnEnable()
    {
        _playerInputs.Enable();
    }

    private void OnDisable()
    {
        _playerInputs.Disable();
    }


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        MainCamera.transform.LookAt(CameraRoot);
        _originalCameraLocalPosition = MainCamera.transform.localPosition;
        _maxCameraDistance = Vector3.Distance(CameraRoot.transform.position, MainCamera.transform.position);

        StartIdle();
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseMenu.GameIsPaused) { return; }
        RunCamera();
        RunGravity();
        RunStates();
        Move();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 v = CameraRoot.forward;
        v.y = 0;
        Gizmos.DrawLine(transform.position, transform.position+v.normalized);
        Gizmos.color = Color.yellow;
        /*
         * If an enemy is targeted, draw a sphere at the position
         * 
         * 
         * 
         * 
         */
    }
    void RunCamera()
    {
        //Calculate Camera Rotation based of mouse movement
        _targetRotationH += _inputLook.x * CameraSensitivityX * Time.deltaTime;
        _targetRotationV += _inputLook.y * CameraSensitivityY * Time.deltaTime;

        //Clamp Vertical Rotation
        _targetRotationV = Mathf.Clamp(_targetRotationV, MaxUpwardAngle, MaxDownwardAngle);

        CameraRoot.rotation = Quaternion.Euler(_targetRotationV, _targetRotationH, 0.0f);
        //Check for Environement Collision
        int layerMask = 1 << 6;
        layerMask = ~layerMask;
        RaycastHit hit;
        Vector3 dir = MainCamera.transform.position - CameraRoot.transform.position;
        bool collided = Physics.Raycast(CameraRoot.transform.position, dir.normalized, out hit, _maxCameraDistance, layerMask);
        if (collided && hit.collider.tag != "Player")
        {
            MainCamera.transform.localPosition = CameraRoot.transform.InverseTransformPoint(hit.point);
        }
        else
        {
            MainCamera.transform.localPosition = _originalCameraLocalPosition;
        }
    }

    void RunGravity()
    {
        if (!_playerController.isGrounded)
        {
            _targetVelocityY += Gravity * Time.deltaTime;
            if (_targetVelocityY < _terminalVelocityY)
            {
                _targetVelocityY = _terminalVelocityY;
            }
        }
        else
        {
            _targetVelocityY = -1f;
        }
    }

    void RunStates()
    {
        switch (_currentState)
        {
            case PlayerStates.Idle:
                Idle();
                break;
            case PlayerStates.Jump:
                Jump();
                break;
            case PlayerStates.Run:
                Run();
                break;
            case PlayerStates.Roll:
                Roll();
                break;
            case PlayerStates.Fall:
                Fall();
                break;
            case PlayerStates.Attack:
                Attack();
                break;
            case PlayerStates.Hurt:
                Hurt();
                break;
        }
        //Debug.Log("Current State : " + _currentState);
    }

    private void StartIdle() 
    {
        _currentState = PlayerStates.Idle;
        _playerAnimator.Play("Idle");
        ClearMotion();
    }
    private void Idle() 
    {
        StopIdle();
    }
    private void StopIdle()
    {
        if (!_playerController.isGrounded) { StartFall(); }
        else if (_inputMove.x != 0 || _inputMove.y != 0) { StartRun(); }
        else if (_inputJump) { StartJump(); }
        else if (_inputAttack) { StartAttack(); }
    }

    private void StartJump() 
    {
        _currentState = PlayerStates.Jump;
        _playerAnimator.Play("Jump");
        _targetVelocityY = Mathf.Sqrt(JumpHeight * -2f * Gravity);
        _inputJump = false;
        Debug.Log("Current State : " + _currentState);
    }
    private void Jump()
    {
        StopJump();
    }
    private void StopJump() 
    {
        if(_targetVelocityY <= 0)
        {
            if (!_playerController.isGrounded)
            {
                StartFall();
            }
            else
            {
                StartIdle();
            }
        }
    }

    private void StartRun()
    {
        _currentState = PlayerStates.Run;
        _playerAnimator.Play("Run");
        Debug.Log("Current State : " + _currentState);
    }
    private void Run()
    {
        Vector3 newDirection = new Vector3(_inputMove.x, 0f, _inputMove.y);
        if (newDirection == Vector3.zero)
        {
            _targetVelocityXZ = Mathf.Lerp(_targetVelocityXZ, 0, Time.deltaTime * SpeedChangeFactor); //10 * 1 * 0.5
            if (_targetVelocityXZ < 0.1f)
            {
                _targetVelocityXZ = 0;
            }
        }
        else
        {
            _targetVelocityXZ = Mathf.Lerp(_targetVelocityXZ, MaxSpeed, Time.deltaTime * SpeedChangeFactor);
            if (_targetVelocityXZ > MaxSpeed - 0.1f)
            {
                _targetVelocityXZ = MaxSpeed;
            }
            //Use CameraRoots rotation, making forward always in front of the camera, modified based on the inputs
            _moveDirection = CameraRoot.rotation * newDirection;
            _moveDirection.y = 0;
        }

        RotateCharacter();

        SetMotion(_targetVelocityXZ);

        StopRun();
    }
    private void StopRun()
    {
        if(_targetVelocityXZ == 0)
        {
            StartIdle();
        }
        else if (_inputJump)
        {
            StartRoll();
        }
        else if (_inputAttack) 
        { 
            StartAttack(); 
        }
        else if (!_playerController.isGrounded)
        {
            StartFall();
        }
    }

    private void StartRoll()
    {
        _currentState = PlayerStates.Roll;
        _playerAnimator.Play("Roll");
        _inputJump = false;
    }
    private void Roll()
    {
        _targetVelocityXZ = Mathf.Lerp(_targetVelocityXZ, RollSpeed, Time.deltaTime * 25);
        if (_targetVelocityXZ > RollSpeed - 0.1f)
        {
            _targetVelocityXZ = RollSpeed;
        }

        SetMotion(_targetVelocityXZ);

        StopRoll();
    }
    private void StopRoll()
    {
        if (IsAnimatorMatchingState("Roll"))
        {
            if (!IsAnimatorPlaying())
            {
                StartRun();
            }
            else if (!_playerController.isGrounded)
            {
                SetMotion(JumpSpeed);
                StartJump();
            }
        }
    }

    private void StartFall() 
    {
        _currentState = PlayerStates.Fall;
        _playerAnimator.Play("Fall");
        Debug.Log("Current State : " + _currentState);
    }
    private void Fall()
    {
        StopFall();
    }
    private void StopFall()
    {
        if (_playerController.isGrounded)
        {
            StartIdle();
        }
    }

    private void StartAttack()
    {
        _currentState = PlayerStates.Attack;
        _playerAnimator.Play("Attack");
        _playerAnimator.SetInteger("AttackLevel", 1);
        //CheckEnemy ? Default to where the camera points
        _moveDirection = CameraRoot.rotation * Vector3.forward;
        _moveDirection.y = 0;
        _inputAttack = false;
        RotateCharacter();
        ClearMotion();
    }
    private void Attack()
    {
        if (IsAnimatorMatchingState("Attack"))
        {
            if (!IsAnimatorPlaying())
            {
                StartIdle();
            }
            else if (_inputAttack)
            {
                _playerAnimator.SetInteger("AttackLevel",2);
                _inputAttack = false;
            }
        }
        else if (IsAnimatorMatchingState("Attack2"))
        {
            if (!IsAnimatorPlaying())
            {
                StartIdle();
            }
        }
    }
    private void StopAttack()
    {

    }

    private void StartHurt()
    {

    }
    private void Hurt()
    {

    }
    private void StopHurt()
    {

    }
    

    void Move()
    {
        _moveMotion.y = _targetVelocityY;
        _playerController.Move(_moveMotion * Time.deltaTime);
    }

    void SetMotion(float speed)
    {
        _moveMotion.x = _moveDirection.x * speed;
        _moveMotion.z = _moveDirection.z * speed;
    }

    void ClearMotion()
    {
        _moveMotion.x = 0;
        _moveMotion.z = 0;
    }

    void RotateCharacter()
    {
        RootGeometry.transform.LookAt(_playerController.transform.position + _moveDirection);
    }

    private bool IsAnimatorPlaying()
    {
        return _playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
    }
    private bool IsAnimatorMatchingState(string stateName)
    {
        return _playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }
}
