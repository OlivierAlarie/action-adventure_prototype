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
    public float MaxDashSpeed = 10f;
    public float MaxSpeed = 5f; //Target speed for the character
    public float TimeToMaxSpeed = 1f;//Time to reach max speed (in Seconds)
    public float JumpHeight = 1f; //Target height for the character
    public float JumpLeniency = 0.15f; // Seconds of leniency where a jump attempt is registered before reaching the ground / after leaving the ground without jumping
    public float Gravity = -9.81f;
    public float MaxFallHeight = 25; //If the character falls MaxFallHeight units on the y axis between 2 grounded checks, he dies
    public Transform LastCheckpoint;

    public AudioSource _jumpSound;
    public AudioSource _landingSound;

    [Header("Model")]
    public Transform RootGeometry;

    //Inputs
    private Vector2 _inputMove;
    private Vector2 _inputLook;
    private bool _inputJump;
    private bool _inputDash;

    private Vector3 _targetDirection = Vector3.zero;
    private float _targetVelocityXZ = 0f;
    private float _targetVelocityChangeRate = 0f;
    private float _targetVelocityY = 0f;
    private float _targetSpeed = 0f;
    private float _terminalVelocityY = -53f;
    private bool _playerJumping = false;
    private bool _playerFalling = false;
    private float _playerJumpTimer = 0f;
    private float _playerCoyoteTimer = 0f;
    private float _lastHeight = 0f;

    private float _knockBackCounter = 0f;

    private PlayerInput _playerInputs;

    private void Awake()
    {
        _playerInputs = new PlayerInput();

        _playerInputs.CharacterControls.Move.started += OnMoveInput;
        _playerInputs.CharacterControls.Move.performed += OnMoveInput;
        _playerInputs.CharacterControls.Move.canceled += OnMoveInput;

        _playerInputs.CharacterControls.Jump.started += OnJumpInput;
        _playerInputs.CharacterControls.Jump.performed += OnJumpInput;
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
        Debug.Log(_inputMove);
    }

    private void OnLookInput(InputAction.CallbackContext context)
    {
        _inputLook = context.ReadValue<Vector2>();
        Debug.Log(_inputLook);

    }

    private void OnJumpInput(InputAction.CallbackContext context)
    {
        _inputJump = context.ReadValueAsButton();
    }

    private void OnDashInput(InputAction.CallbackContext context)
    {
        _inputDash = false;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        MainCamera.transform.LookAt(CameraRoot);
        _originalCameraLocalPosition = MainCamera.transform.localPosition;
        _maxCameraDistance = Vector3.Distance(CameraRoot.transform.position, MainCamera.transform.position);

        _targetVelocityChangeRate = 1 / TimeToMaxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PauseMenu.GameIsPaused)
        {
            CameraCheck();
            JumpAndGravityCheck();
            MovePlayer();
            if(_characterAnimator != null) AnimatePlayer();
        }
    }

    void CameraCheck()
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

    void JumpAndGravityCheck()
    {
        if (_characterController.isGrounded)
        {
            _targetVelocityY = 0;
            _playerJumping = false;
            if (_playerFalling)
            {
                _landingSound.Play();
                _playerFalling = false;
            }
            if (_inputJump || _playerJumpTimer > 0)
            {
                _jumpSound.Play();
                _targetVelocityY = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                _playerJumping = true;
            }
            _lastHeight = transform.position.y;
            _playerJumpTimer = 0;
            _playerCoyoteTimer = JumpLeniency;
        }
        else
        {
            //Coyote Time
            //Early jump stop
            if (!_inputJump && _playerJumping && _targetVelocityY > 0)
            {
                _targetVelocityY *= 0.5f;
            }

            //Jump Leniency
            if (_inputJump)
            {
                if (_playerCoyoteTimer > 0)
                {
                    _targetVelocityY = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    _jumpSound.Play();
                    _playerJumping = true;
                }
                else
                {
                    _playerJumpTimer = JumpLeniency;
                }
            }
            _playerCoyoteTimer -= Time.deltaTime;
            _playerJumpTimer -= Time.deltaTime;

            _targetVelocityY += Gravity * Time.deltaTime;
            if (_targetVelocityY < _terminalVelocityY)
            {
                _targetVelocityY = _terminalVelocityY;
            }
        }
    }

    void MovePlayer()
    {
        Vector3 newDirection = Vector3.zero;

        //Check if being knockbacked
        if (_knockBackCounter <= 0)
        {
            newDirection = new Vector3(_inputMove.x, 0f, _inputMove.y);

            if (_characterController.isGrounded)
            {
                _targetSpeed = _inputDash ? MaxDashSpeed : MaxSpeed;
            }
        }
        else
        {
            _knockBackCounter -= Time.deltaTime;
        }
       
        if (newDirection == Vector3.zero)
        {
            _targetVelocityXZ -= _targetSpeed * _targetVelocityChangeRate * Time.deltaTime; //10 * 1 * 0.5
            if (_targetVelocityXZ < 0)
            {
                _targetVelocityXZ = 0;
            }
        }
        else
        {
            _targetVelocityXZ += _targetSpeed * _targetVelocityChangeRate * Time.deltaTime;
            if (_targetVelocityXZ > _targetSpeed)
            {
                _targetVelocityXZ = _targetSpeed;
            }
            //Use CameraRoots rotation, making forward always in front of the camera
            _targetDirection = Quaternion.Euler(0.0f, CameraRoot.rotation.eulerAngles.y, 0.0f) * newDirection;
            RootGeometry.transform.LookAt(_characterController.transform.position + _targetDirection);
        }

        //Keep the direction to magnitude 1
        _targetDirection.y = 0;
        _targetDirection.Normalize();
        //Multiply by whatever currentVelocity we target
        _targetDirection *= _targetVelocityXZ;
        _targetDirection.y = _targetVelocityY;
        
        _characterController.Move(_targetDirection * Time.deltaTime);

    }

    private void AnimatePlayer()
    {

        if (_characterController.isGrounded)
        {
            _targetVelocityY = 0;
            _characterAnimator.SetBool("isJumping", false);
            if (_targetVelocityXZ <= 0)
            {
                _characterAnimator.SetBool("isJogging", false);
                _characterAnimator.SetBool("isRunning", false);
            }
            else if (_targetVelocityXZ <= MaxSpeed)
            {
                _characterAnimator.SetBool("isJogging", true);
                _characterAnimator.SetBool("isRunning", false);
            }
            else if (_targetVelocityXZ <= MaxDashSpeed)
            {
                _characterAnimator.SetBool("isRunning", true);
            }
        }

        if (_targetVelocityY > 0)
        {
            _characterAnimator.SetBool("isJumping", true);
        }

        if(_targetVelocityY <= -7)
        {
            _playerFalling = true;
        }

    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

    }

    public void KnockBack(Vector3 knockBackDirection, float knockBackDuration = 0.5f, float knockBackForce = 10f)
    {
        if(_knockBackCounter <= 0)
        {
            _targetDirection = knockBackDirection;
            _knockBackCounter = knockBackDuration;
            _targetVelocityXZ = knockBackForce;
        }
    }
}
