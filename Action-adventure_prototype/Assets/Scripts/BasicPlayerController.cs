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
    public Animator _characterAnimator;
    public CharacterController _characterController;
    public float MaxSpeed = 5f; //Target speed for the character
    public float TimeToMaxSpeed = 1f;//Time to reach max speed (in Seconds)
    public float RollLength = 10f;
    public float JumpHeight = 1f; //Target height for the character
    public float JumpLeniency = 0.15f; // Seconds of leniency where a jump attempt is registered before reaching the ground / after leaving the ground without jumping
    public float Gravity = -9.81f;
    public Transform LastCheckpoint;

    [Header("Model")]
    public Transform RootGeometry;

    //Inputs
    private PlayerInput _playerInputs;
    private Vector2 _inputMove;
    private Vector2 _inputLook;
    private bool _inputJump = false;

    private Vector3 _targetDirection = Vector3.zero;
    private Vector3 _targetMotion = Vector3.zero;
    private float _targetVelocityXZ = 0f;
    private float _targetVelocityChangeRate = 0f;
    private float _targetVelocityY = 0f;
    private float _terminalVelocityY = -53f;

    private PlayerStates _currentState;

    private enum PlayerStates
    {
        Idle,
        Jump,
        Run,
        Roll,
        Fall
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
    }

    private void OnEnable()
    {
        _playerInputs.Enable();
    }

    private void OnDisable()
    {
        _playerInputs.Disable();
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
        _inputJump = false;
        if (context.action.WasPressedThisFrame())
        {
            Debug.Log("Jump Pressed");
            _inputJump = true;
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        MainCamera.transform.LookAt(CameraRoot);
        _originalCameraLocalPosition = MainCamera.transform.localPosition;
        _maxCameraDistance = Vector3.Distance(CameraRoot.transform.position, MainCamera.transform.position);

        _targetVelocityChangeRate = 1 / TimeToMaxSpeed;
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
    void RunCamera()
    {
        //Calculate Camera Rotation based of mouse movement
        _targetRotationH += _inputLook.x * CameraSensitivityX * Time.deltaTime;
        _targetRotationV += _inputLook.y * CameraSensitivityY * Time.deltaTime;

        //Clamp Vertical Rotation
        _targetRotationV = Mathf.Clamp(_targetRotationV, MaxUpwardAngle, MaxDownwardAngle);

        CameraRoot.transform.rotation = Quaternion.Euler(_targetRotationV, _targetRotationH, 0.0f);
        //Check for Environement Collision
        int layerMask = 1 << 6;
        layerMask = ~layerMask;
        RaycastHit hit;
        Vector3 dir = MainCamera.transform.position - CameraRoot.transform.position;
        bool collided = Physics.Raycast(CameraRoot.transform.position, dir.normalized, out hit, _maxCameraDistance, layerMask);
        if (collided && hit.collider.name != "PlayerCharacter")
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
        if (!_characterController.isGrounded)
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
        }
        //Debug.Log(_currentState);
    }

    private void StartIdle() 
    {
        _currentState = PlayerStates.Idle;
        _characterAnimator.Play("Idle");
        _targetMotion.x = 0;
        _targetMotion.z = 0;
    }
    private void Idle() 
    {
        StopIdle();
    }
    private void StopIdle()
    {
        if (!_characterController.isGrounded) { StartFall(); }
        else if (_inputMove.x != 0 || _inputMove.y != 0) { StartRun(); }
        else if (_inputJump) { StartJump(); }
    }

    private void StartJump() 
    {
        _currentState = PlayerStates.Jump;
        _characterAnimator.Play("Jump");
        _targetVelocityY = Mathf.Sqrt(JumpHeight * -2f * Gravity);

    }
    private void Jump()
    {
        StopJump();
    }
    private void StopJump() 
    {
        if(_targetVelocityY <= 0)
        {
            if (!_characterController.isGrounded)
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
        _characterAnimator.Play("Run");
    }
    private void Run()
    {
        StopRun();

        Vector3 newDirection = new Vector3(_inputMove.x, 0f, _inputMove.y);
        if (newDirection == Vector3.zero)
        {
            _targetVelocityXZ -= MaxSpeed * _targetVelocityChangeRate * Time.deltaTime; //10 * 1 * 0.5
            if (_targetVelocityXZ <= 0)
            {
                _targetVelocityXZ = 0;
            }
        }
        else
        {
            _targetVelocityXZ += MaxSpeed * _targetVelocityChangeRate * Time.deltaTime;
            if (_targetVelocityXZ > MaxSpeed)
            {
                _targetVelocityXZ = MaxSpeed;
            }
            //Use CameraRoots rotation, making forward always in front of the camera
            _targetDirection = Quaternion.Euler(0.0f, CameraRoot.rotation.eulerAngles.y, 0.0f) * newDirection;
            RootGeometry.transform.LookAt(_characterController.transform.position + _targetDirection);
        }

        _targetMotion.x = _targetDirection.x * _targetVelocityXZ;
        _targetMotion.z = _targetDirection.z * _targetVelocityXZ;
    }
    private void StopRun()
    {
        if(_targetVelocityXZ == 0)
        {
            StartIdle();
        }
        if (_inputJump)
        {
            StartRoll();
        }
    }

    private void StartRoll()
    {
        _currentState = PlayerStates.Roll;
        _characterAnimator.Play("Roll");
    }
    private void Roll()
    {
        StopRoll();
        _targetMotion.x = _targetDirection.x * RollLength;
        _targetMotion.z = _targetDirection.z * RollLength;
    }
    private void StopRoll()
    {
        StartRun();
    }

    private void StartFall() 
    {
        _currentState = PlayerStates.Fall;
        _characterAnimator.Play("Fall");
    }
    private void Fall()
    {
        StopFall();
    }
    private void StopFall()
    {
        if (_characterController.isGrounded)
        {
            StartIdle();
        }
    }

    //LastSteps
    void Move()
    {
        _targetMotion.y = _targetVelocityY;
        _characterController.Move(_targetMotion * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

    }
}
