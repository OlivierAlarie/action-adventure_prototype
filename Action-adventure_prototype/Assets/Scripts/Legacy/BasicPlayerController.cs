using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    // Start is called before the first frame update
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
        if (Time.timeScale != 0)
        {
            CameraCheck();
            JumpAndGravityCheck();
            MovePlayer();
            AnimatePlayer();
        }
    }

    void CameraCheck()
    {
        //Calculate Camera Rotation based of mouse movement
        _targetRotationH += Input.GetAxis("Camera X") * CameraSensitivityX * Time.deltaTime;
        _targetRotationV += Input.GetAxis("Camera Y") * CameraSensitivityY * Time.deltaTime;

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
            if (Input.GetButtonDown("Jump") || _playerJumpTimer > 0)
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
            if (Input.GetButtonUp("Jump") && _playerJumping && _targetVelocityY > 0)
            {
                _targetVelocityY *= 0.5f;
            }

            //Jump Leniency
            if (Input.GetButtonDown("Jump"))
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
            if(_lastHeight - transform.position.y > MaxFallHeight)
            {
                Debug.Log("Dead");
            }
        }
    }

    void MovePlayer()
    {
        Vector3 newDirection = Vector3.zero;

        //Check if being knockbacked
        if (_knockBackCounter <= 0)
        {
            newDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

            if (_characterController.isGrounded)
            {
                _targetSpeed = Input.GetButton("Dash") ? MaxDashSpeed : MaxSpeed;
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
        GameObject gameO = hit.collider.gameObject;
        if (gameO.tag == "Wraith")
        {
            KnockBack(hit.normal, gameO.GetComponent<Wraith>().knockBackDuration, gameO.GetComponent<Wraith>().knockBackForce);
        }
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
