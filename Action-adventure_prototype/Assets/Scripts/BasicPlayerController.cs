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
    private bool _cameraLocked;

    [Header("Character")]
    public Animator _playerAnimator;
    public CharacterController _playerController;
    public int Health = 6;
    public int NumberOfKeys = 0;
    public float MaxSpeed = 5f; //Target speed for the character
    public float SpeedChangeFactor = 5f;//Acceleration/Decceleration
    public float RollSpeed = 10f;//Target speed during roll
    public float JumpSpeed = 25f;
    public float JumpHeight = 1f; //Target height for the character
    public float JumpLeniency = 0.15f; // Seconds of leniency where a jump attempt is registered before reaching the ground / after leaving the ground without jumping
    public float WallRunHeight = 1f;
    public float Gravity = -9.81f;
    public float MaxDistanceTargetLock; //Max distance to acquire target
    public float DistanceFromTarget;//Distance to reach from target when attacking
    public bool CanPushSideways;
    public Transform LastCheckpoint;

    [Header("Model")]
    public Transform RootGeometry;

    //Inputs
    private PlayerInput _playerInputs;
    private Vector2 _inputMove;
    private Vector2 _inputLook;
    private bool _inputRoll;
    private bool _inputAttack;
    private bool _inputBlock;

    private Vector3 _moveDirection = Vector3.zero;
    private Vector3 _moveMotion = Vector3.zero;
    [SerializeField] private float _targetVelocityXZ = 0f;
    private float _targetVelocityY = 0f;
    private float _terminalVelocityY = -53f;
    private float _wallRunTimer = 0f;
    private string _wallRunAnimation;
    private float _jumpHangTimer = 0f;
    private float _hurtTimer = 0f;
    private GameObject _currentTarget;
    private PushableObject _pushableObject;
    private Vector3 _lastHurtDirection;

    private PlayerStates _currentState;

    private enum PlayerStates
    {
        Idle,
        Jump,
        Run,
        Roll,
        Fall,
        Attack,
        Hurt,
        Push,
        WallRun
    }

    private void Awake()
    {
        _playerInputs = new PlayerInput();

        _playerInputs.CharacterControls.Move.started += OnMoveInput;
        _playerInputs.CharacterControls.Move.performed += OnMoveInput;
        _playerInputs.CharacterControls.Move.canceled += OnMoveInput;

        _playerInputs.CharacterControls.Roll.started += OnRollInput;
        _playerInputs.CharacterControls.Roll.canceled += OnRollInput;

        _playerInputs.CharacterControls.Look.started += OnLookInput;
        _playerInputs.CharacterControls.Look.performed += OnLookInput;
        _playerInputs.CharacterControls.Look.canceled += OnLookInput;

        _playerInputs.CharacterControls.Attack.started += OnAttackInput;
        _playerInputs.CharacterControls.Attack.canceled += OnAttackInput;

        _playerInputs.CharacterControls.Block.started += OnBlockInput;
        _playerInputs.CharacterControls.Block.canceled += OnBlockInput;
    }

    #region Inputs
    private void OnMoveInput(InputAction.CallbackContext context)
    {
        _inputMove = context.ReadValue<Vector2>();
    }
    private void OnLookInput(InputAction.CallbackContext context)
    {
        _inputLook = context.ReadValue<Vector2>();
    }
    private void OnRollInput(InputAction.CallbackContext context)
    {
        _inputRoll = context.ReadValueAsButton();
    }
    private void OnAttackInput(InputAction.CallbackContext context)
    {
        _inputAttack = context.ReadValueAsButton();
    }
    private void OnBlockInput(InputAction.CallbackContext context)
    {
        _inputBlock = context.ReadValueAsButton();
    }
    private void OnEnable()
    {
        _playerInputs.Enable();
    }
    private void OnDisable()
    {
        _playerInputs.Disable();
    }
    #endregion

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
        if (GameMaster.Instance != null && GameMaster.Instance.GameIsPaused) { return; }

        if(Health <= 0) { return; }

        RunCamera();

        RunTargeter();

        RunGravity();

        RunStates();

        if(!_playerController.enabled) { return; }
        Move();
    }

    private void OnDrawGizmosSelected()
    {
        
        Vector3 v = CameraRoot.forward;
        v.y = 0;
        v.Normalize();
        if(_currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position,_currentTarget.transform.position-transform.position);
            Gizmos.DrawWireSphere(_currentTarget.transform.position, 1);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, v);
        }
    }
    void RunCamera()
    {
        if (_cameraLocked) return;
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

    void RunTargeter()
    {
        if(_currentState == PlayerStates.Attack) { return; }
        //Grab Nearest Enemy
        RaycastHit hit;
        Vector3 v = CameraRoot.forward;
        v.y = 0;
        v.Normalize();
        //Catch Only Enemy layer
        int lm = 1 << 7;
        bool enemyCollided = Physics.SphereCast(transform.position, 0.5f, v, out hit, MaxDistanceTargetLock,lm);
        //Catch everything but things that ignore the camera
        lm = 1 << 6;
        lm = ~lm;
        bool obstructed = Physics.Raycast(transform.position, v, hit.distance,lm);
        if (enemyCollided)
        {
            _currentTarget = hit.collider.gameObject;
        }
        if (obstructed)
        {
            _currentTarget = null;
        }
        if (_currentTarget != null)
        {
            if (Vector3.Distance(transform.position, _currentTarget.transform.position) > MaxDistanceTargetLock)
            {
                _currentTarget = null;
            }
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
                Health = 0;
            }
        }
        else
        {
            _targetVelocityY = -10f;
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
            case PlayerStates.Push:
                Push();
                break;
            case PlayerStates.WallRun:
                WallRun();
                break;
        }
    }

    private void StartIdle() 
    {
        _currentState = PlayerStates.Idle;
        _playerAnimator.Play("Idle");
        ClearHorizontalMotion();
    }
    private void Idle() 
    {
        StopIdle();
    }
    private void StopIdle()
    {
        if (!_playerController.isGrounded) { StartFall(); }
        else if (_inputMove.x != 0 || _inputMove.y != 0) { StartRun(); }
        else if (_inputRoll) {
            RaycastHit[] hits = Physics.RaycastAll(transform.position, RootGeometry.transform.forward, 0.6f);
            foreach (var hit in hits)
            {
                if (hit.collider.isTrigger && hits[0].collider.tag == "PushableObject")
                {
                    _pushableObject = hit.collider.GetComponent<PushableObject>();
                    StartPush();
                }
            }
        }
        else if (_inputAttack) { StartAttack(); }
    }

    private void StartJump() 
    {
        _currentState = PlayerStates.Jump;
        _playerAnimator.Play("Jump");
        _targetVelocityY = Mathf.Sqrt(JumpHeight * -2f * Gravity);
        _inputRoll = false;
        _jumpHangTimer = -1f;
    }
    private void Jump()
    {
        RaycastHit hit;
        int lm = 1 << 6;
        lm = ~lm;
        Ray[] rays = new Ray[4];
        rays[0] = new Ray(transform.position, Vector3.forward);
        rays[1] = new Ray(transform.position, Vector3.back);
        rays[2] = new Ray(transform.position, Vector3.left);
        rays[3] = new Ray(transform.position, Vector3.right);
        foreach (var ray in rays)
        {
            if (Physics.Raycast(ray, out hit, 1f, lm) && _jumpHangTimer == -1f)
            {
                if (Vector3.Angle(hit.normal * -1, RootGeometry.transform.forward) < 35)
                {
                    ClearHorizontalMotion();
                    _playerController.enabled = false;
                    transform.position = hit.point + (hit.normal * 0.5f);
                    _playerController.enabled = true;
                    _jumpHangTimer = 0.35f;
                    _playerAnimator.Play("Hang");
                    break;
                }
            }
        }


        if (_jumpHangTimer > 0)
        {
            _targetVelocityY = 0;
            _jumpHangTimer -= Time.deltaTime;
        }

        StopJump();
    }
    private void StopJump() 
    {
        if(_targetVelocityY < 0 && _jumpHangTimer <= 0)
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
        else if(_jumpHangTimer > 0 && _inputRoll)
        {
            _moveDirection = RootGeometry.transform.forward * -1;
            _moveDirection.y = 0;
            RootGeometry.transform.LookAt(transform.position + _moveDirection);
            StartRoll();
        }
    }

    private void StartRun()
    {
        _currentState = PlayerStates.Run;
        _playerAnimator.Play("Run");
    }
    private void Run()
    {
        Vector3 newDirection = new Vector3(_inputMove.x, 0f, _inputMove.y);
        if (newDirection == Vector3.zero)
        {
            _targetVelocityXZ = Mathf.Lerp(_targetVelocityXZ, 0, Time.deltaTime * SpeedChangeFactor); //10 * 1 * 0.5
            if (_targetVelocityXZ < 0.5f)
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

        SetHorizontalMotion(_targetVelocityXZ);

        RootGeometry.transform.LookAt(_playerController.transform.position + _moveDirection);

        StopRun();
    }
    private void StopRun()
    {
        if(_targetVelocityXZ == 0)
        {
            StartIdle();
        }
        else if (_inputRoll)
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position, RootGeometry.transform.forward, 0.6f);
            foreach (var hit in hits)
            {
                if (hit.collider.isTrigger && hit.collider.tag == "PushableObject")
                {
                    _pushableObject = hit.collider.GetComponent<PushableObject>();
                    StartPush();
                }
            }
            
            if(_pushableObject == null)
            {
                StartRoll();
            }
        }
        else if (_inputAttack) 
        { 
            StartAttack(); 
        }
        else if (_inputBlock)
        {
            //RayCast towards direction
            int lm = 1 << 8;
            RaycastHit hit;
            Ray[] rays = new Ray[4];
            rays[0] = new Ray(transform.position, Vector3.forward);
            rays[1] = new Ray(transform.position, Vector3.back);
            rays[2] = new Ray(transform.position, Vector3.left);
            rays[3] = new Ray(transform.position, Vector3.right);
            foreach (var ray in rays)
            {
                if(Physics.Raycast(ray, out hit, 1f,lm))
                {

                    if(Vector3.Angle(hit.normal * -1, RootGeometry.transform.forward) > 75) { break; }


                    if (Vector3.Angle(hit.normal * -1, RootGeometry.transform.forward) < 35)
                    {
                        ClearHorizontalMotion();
                        _targetVelocityY = Mathf.Sqrt(WallRunHeight * -2f * Gravity);
                        _wallRunAnimation = "WallRun";
                    }
                    else if (Vector3.Angle(hit.normal * -1, RootGeometry.transform.forward) <= 75)
                    {
                        Vector3 v = Vector3.Cross(hit.normal * -1, RootGeometry.transform.forward);
                        if(v.y < 0)
                        {
                            _moveDirection = (Quaternion.Euler(0f, 90f, 0f) * hit.normal);
                            _moveDirection.y = 0;
                            _moveDirection.Normalize();
                            _wallRunAnimation = "WallRunL";
                        }
                        else
                        {
                            _moveDirection = (Quaternion.Euler(0f, -90f, 0f) * hit.normal);
                            _moveDirection.y = 0;
                            _moveDirection.Normalize();
                            _wallRunAnimation = "WallRunR";
                        }
                        SetHorizontalMotion(JumpSpeed);
                        _targetVelocityY = Mathf.Sqrt(WallRunHeight*0.75f * -2f * Gravity);
                    }

                    RootGeometry.transform.LookAt(transform.position + -hit.normal);
                    _playerController.enabled = false;
                    transform.position = hit.point + (hit.normal * 0.5f);
                    _playerController.enabled = true;

                    StartWallRun();
                    break;
                }
            }
        }
        else if (!_playerController.isGrounded)
        {
            StartFall();
        }
    }

    private void StartWallRun()
    {
        _currentState = PlayerStates.WallRun;
        Vector3.Angle(RootGeometry.transform.forward, _moveDirection);
        _playerAnimator.Play(_wallRunAnimation);
        _inputBlock = false;
        _wallRunTimer = 0f;
    }
    private void WallRun()
    {
        _wallRunTimer += Time.deltaTime;
        StopWallRun();
    }
    private void StopWallRun()
    {
        if (_wallRunTimer >= 1f || !Physics.Raycast(transform.position, RootGeometry.forward, 2.5f))
        {
            _targetVelocityY *= 0.5f;
            StartFall();
        }
        else if (_inputRoll)
        {
            _moveDirection = RootGeometry.transform.forward * -1;
            _moveDirection.y = 0;
            RootGeometry.transform.LookAt(transform.position+_moveDirection);
            StartRoll();
        }
        else if (_playerController.isGrounded)
        {
            StartIdle();
        }
    }

    private void StartRoll()
    {
        _currentState = PlayerStates.Roll;
        _playerAnimator.Play("Roll");
        _inputRoll = false;
    }
    private void Roll()
    {
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
            else if (!_playerController.isGrounded && !Physics.Raycast(transform.position, Vector3.down, 2f))
            {
                if(_playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.25f)
                {
                    SetHorizontalMotion(JumpSpeed);
                    StartJump();
                }
                else
                {
                    StartFall();
                }

            }
        }
    }

    private void StartFall() 
    {
        _currentState = PlayerStates.Fall;
        _playerAnimator.Play("Fall");
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
        ClearHorizontalMotion();
        AttackDirection();
    }
    private void Attack()
    {
        if(_currentTarget != null && Vector3.Distance(_currentTarget.transform.position,transform.position) <= DistanceFromTarget)
        {
            ClearHorizontalMotion();
        }
        StopAttack();
    }
    private void AttackDirection()
    {
        Vector3 movedirection = new Vector3(_inputMove.x, 0, _inputMove.y) == Vector3.zero ? Vector3.forward : new Vector3(_inputMove.x, 0, _inputMove.y);
        _moveDirection = CameraRoot.rotation * movedirection;
        _moveDirection.y = 0;
        _inputAttack = false;
        RootGeometry.transform.LookAt(_playerController.transform.position + _moveDirection);
    }
    private void StopAttack()
    {
        if (IsAnimatorMatchingState("Attack"))
        {
            if (_playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.65f && _inputAttack)
            {
                _playerAnimator.Play("Attack2");
                AttackDirection();
            }

        }
        else if (IsAnimatorMatchingState("Attack2"))
        {
            if (_playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.65f && _inputAttack)
            {
                _playerAnimator.Play("Attack3");
                AttackDirection();
            }
        }
        else if (IsAnimatorMatchingState("Attack3"))
        {
            if (_playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.65f && _inputAttack)
            {
                _playerAnimator.Play("Attack");
                AttackDirection();
            }
        }


        if (!IsAnimatorPlaying())
        {
            StartIdle();
        }
    }

    private void StartHurt()
    {
        _currentState = PlayerStates.Hurt;
        _playerAnimator.Play("Hurt");
        _hurtTimer = 0f;
        _moveDirection = _lastHurtDirection;
        ClearHorizontalMotion();
    }
    private void Hurt()
    {
        _hurtTimer += Time.deltaTime;
        StopHurt();
    }
    private void StopHurt()
    {
        if(_hurtTimer >= 0.5f)
        {
            StartIdle();
        }
    }
    
    private void StartPush()
    {
        _currentState = PlayerStates.Push;
        _playerAnimator.Play("Grab", 0, 1f);
        _cameraLocked = true;
        Vector3 v = _pushableObject.transform.position;
        Vector3 s = _pushableObject.transform.localScale/2;

        //check where the character is compared to the pushable object

        if(v.x+s.x < transform.position.x)
        {
            RootGeometry.transform.LookAt(Vector3.left+transform.position);
        }
        else if(v.x-s.x > transform.position.x)
        {
            RootGeometry.transform.LookAt(Vector3.right+transform.position);
        }
        else if(v.z+s.z < transform.position.z)
        {
            RootGeometry.transform.LookAt(Vector3.back + transform.position);
        }
        else if(v.z-s.z > transform.position.z)
        {
            RootGeometry.transform.LookAt(Vector3.forward + transform.position);
        }

        CameraRoot.rotation = RootGeometry.rotation;
        ClearHorizontalMotion();
        transform.parent = _pushableObject.transform;
        _playerController.enabled = false;
    }
    private void Push()
    {
        Vector3 newDirection = new Vector3(_inputMove.x, 0f, _inputMove.y);
        if(newDirection.z > 0)
        {
            //Push Forward 
            _playerAnimator.Play("Push");
            _pushableObject.Push(RootGeometry.forward, 2f);
        }
        else if(newDirection.z < 0)
        {
            //Pull
            _playerAnimator.Play("Pull");
            _pushableObject.Push(RootGeometry.forward*-1, 4f);
        }
        else if(newDirection.x < 0 && CanPushSideways)
        {
            //Push Left
            _pushableObject.Push(RootGeometry.right*-1, 2f);
        }
        else if(newDirection.x > 0 && CanPushSideways)
        {
            //Push Right
            _pushableObject.Push(RootGeometry.right, 2f);
        }
        else if(!_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Grab") && !_pushableObject.BeingPushed)
        {
            _playerAnimator.Play("Grab",0,1f);
        }

        StopPush();
    }
    private void StopPush()
    {
        if (!_inputRoll)
        {
            transform.parent = null;
            _playerController.enabled = true;
            _cameraLocked = false;
            _pushableObject = null;
            StartIdle();
        }
    }


    void Move()
    {
        _moveMotion.y = _targetVelocityY;
        _playerController.Move(_moveMotion * Time.deltaTime);
    }

    public void SetHorizontalMotion(float speed)
    {
        _moveMotion.x = _moveDirection.x * speed;
        _moveMotion.z = _moveDirection.z * speed;
    }

    public void ClearHorizontalMotion()
    {
        _moveMotion.x = 0;
        _moveMotion.z = 0;
    }

    private bool IsAnimatorPlaying()
    {
        return _playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
    }
    private bool IsAnimatorMatchingState(string stateName)
    {
        return _playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "EnemyWeapon")
        {
            if(_currentState != PlayerStates.Hurt)
            {
                _lastHurtDirection = transform.position - other.GetComponentInParent<Skeleton>().transform.position;
                _lastHurtDirection.y = 0;
                _lastHurtDirection.Normalize();
                StartHurt();
                Health--;
            }
        }
        else if(other.tag == "Key")
        {
            NumberOfKeys++;
            Destroy(other.gameObject);
        }
    }
}
