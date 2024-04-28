using UnityEngine;
using UnityEngine.Serialization;

public class HeroEntity : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody2D _rigidbody;

    [Header("Wall Detection")]
    [SerializeField] private float _wallDetectionDistance = 0.5f;
    [SerializeField] private LayerMask _wallLayer;

    private bool IsTouchingWall()
    {
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, _wallDetectionDistance, _wallLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, _wallDetectionDistance, _wallLayer);

        return hitLeft.collider != null || hitRight.collider != null;
    }

    

    //Camera Follow
    private void Awake()
    {
        _cameraFollowable = GetComponent<CameraFollowable>();
        _cameraFollowable.FollowPositionX = _rigidbody.position.x;
        _cameraFollowable.FollowPositionY = _rigidbody.position.y;
    }

    private void Update()
    {
        _UpdateJumpBuffer();
        _UpdateOrientVisual();
    }

    private void _UpdateCameraFollowablePosition()
    {
        _cameraFollowable.FollowPositionX = _rigidbody.position.x;
        if (IsTouchingGround && !IsJumping)
        {
            _cameraFollowable.FollowPositionY = _rigidbody.position.y;
        }
    }

    private void FixedUpdate()
    {
        _ApplyGroundDetection();
        _UpdateCameraFollowablePosition();

        HeroHorizontalMovementsSettings horizontalMovementSettings = _GetCurrentHorizontalMovementsSettings();
        if (_AreOrientAndMovementOpposite())
        {
            _TurnBack(horizontalMovementSettings);
        }
        else
        {
            _UpdateHorizontalSpeed(horizontalMovementSettings);
            _ChangeOrientFromHorizontalMovement();
        }

        if (IsJumping)
        {
            _UpdateJump();
        }
        else
        {
            if (!IsTouchingGround)
            {
                _ApplyFallGravity(_fallSettings);
            }
            else
            {
                _ResetVerticalSpeed();
                if (_jumpBufferCountdown > 0)
                {
                    JumpStart();
                }
                _jumpCount = 0;
            }
        }

        if (_isDashing)
        {
            _rigidbody.velocity = new Vector2(_orientX * _dashSpeed, 0);
            UpdateDash();
        }
        else
        {
            _ApplyHorizontalSpeed();
            _ApllyVerticalSpeed();
        }

        if (IsTouchingWall() && !IsJumping)
        {
            _horizontalSpeed = 0f;
        }
    }





    #region dash
    [Header("Dash")]
    [SerializeField] private float _dashSpeed = 40f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 2f;
    private float _lastDashTime = -2f; 
    private bool _isDashing;
    private float _dashTimer;

    public bool IsDashing => _isDashing;



    public void StartDash()
    {
        if (_isDashing) return;
        if (Time.time - _lastDashTime < _dashCooldown) return;

        _isDashing = true;
        _dashTimer = _dashDuration;
        _rigidbody.velocity = new Vector2(_orientX * _dashSpeed, 0);
        _rigidbody.gravityScale = 0; 
    }

    private void UpdateDash()
    {
        if (!_isDashing) return;

        _dashTimer -= Time.deltaTime;
        if (_dashTimer <= 0)
        {
            EndDash();
        }
        if (IsTouchingWall())
        {
            EndDash();
        }

    }

    public void EndDash()
    {
        _isDashing = false;
        _rigidbody.gravityScale = 1; 
        _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
        _lastDashTime = Time.time;
    }

    #endregion

    #region jump

    [Header("Jump")]
    [Header("Jump")]
    [SerializeField] private HeroJumpSettings[] _jumpSettings;
    [SerializeField] private HeroFallSettings _jumpFallSettings;
    private int _currentJumpIndex = -1;
    private int _maxJumpCount = 2;
    private int _jumpCount = 0;
    private float _jumpBufferTime = 0.2f;
    private float _jumpBufferCountdown = 0f;
    public bool CanJump => _jumpCount < _maxJumpCount && (_jumpState != JumpState.JumpImpulsion || _jumpTimer >= _jumpSettings[_currentJumpIndex].jumpMinDuration);
    enum JumpState
    {
        NotJumping,
        JumpImpulsion,
        Falling
    }
    private JumpState _jumpState = JumpState.NotJumping;
    private float _jumpTimer = 0f;
    public bool IsJumping => _jumpState != JumpState.NotJumping;

    private void _ApplyFallGravity(HeroFallSettings settings)
    {
        _verticalSpeed -= settings.fallGravity * Time.fixedDeltaTime;
        if (_verticalSpeed < -settings.fallSpeedMax)
        {
            _verticalSpeed = -settings.fallSpeedMax;
        }
    }

    public int GetJumpCount()
    {
        return _jumpCount;
    }

    public bool IsJumpImpulsing => _jumpState == JumpState.JumpImpulsion;
    public bool IsJumpMinDurationReached => _jumpTimer >= _jumpSettings[_currentJumpIndex].jumpMinDuration;


    public void StopJumpImpulsion()
    {
        _jumpState = JumpState.Falling;
    }

    private void _UpdateJumpStateImpulsion()
    {
        _jumpTimer += Time.fixedDeltaTime;
        if (_jumpTimer < _jumpSettings[_currentJumpIndex].jumpMaxDuration)
        {
            _verticalSpeed = _jumpSettings[_currentJumpIndex].jumpSpeed;
        }
        else
        {
            _jumpState = JumpState.Falling;
        }
    }

    private void _UpdateJumpStateFalling()
    {
        if (!IsTouchingGround)
        {
            _ApplyFallGravity(_jumpFallSettings);
        }
        else
        {
            _ResetVerticalSpeed();
            _jumpState = JumpState.NotJumping;
            _jumpCount = 0;
        }
    }


    private void _UpdateJumpBuffer()
    {
        if (_jumpBufferCountdown > 0)
        {
            _jumpBufferCountdown -= Time.deltaTime;
        }
    }

    public void JumpBufferStart()
    {
        _jumpBufferCountdown = _jumpBufferTime;
    }

    private void _UpdateJump()
    {
        switch (_jumpState)
        {
            case JumpState.JumpImpulsion:
                _UpdateJumpStateImpulsion();
                break;
            case JumpState.Falling:
                _UpdateJumpStateFalling();
                break;
        }
    }

    public void JumpStart()
    {
        if (!CanJump || _jumpCount >= _maxJumpCount) return;
        _jumpState = JumpState.JumpImpulsion;
        _jumpTimer = 0f;
        _jumpCount++;
        _currentJumpIndex = _jumpCount - 1;
    }

    #endregion

    #region physics
    [Header("Horizontal Movements")]
    [FormerlySerializedAs(oldName: "_movementsSettings;")]
    [SerializeField] private HeroHorizontalMovementsSettings _groundHorizontalMovementsSettings;
    [SerializeField] private HeroHorizontalMovementsSettings _airHorizontalMovementsSettings;
    private float _horizontalSpeed = 5f;
    private float _moveDirX = 0f;

    [Header("Orientation")]
    [SerializeField] private Transform _orientVisualRoot;
    private float _orientX = 1f;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    [Header("Vertical Movements")]
    private float _verticalSpeed = 0f;

    [Header("Fall")]
    [SerializeField] private HeroFallSettings _fallSettings;

    [Header("Ground")]
    [SerializeField] private GroundDetector _groundDetector;
    public bool IsTouchingGround { get; private set; } = false;
    private CameraFollowable _cameraFollowable;

    private void _ApplyGroundDetection()
    {
        IsTouchingGround = _groundDetector.DetectGroundNearBy();
    }

    private void _ResetVerticalSpeed()
    {
        _verticalSpeed = 0f;
    }

    private void _Accelerate(HeroHorizontalMovementsSettings settings)
    {
        _horizontalSpeed += settings.acceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed > settings.speedMax)
        {
            _horizontalSpeed = settings.speedMax;
        }
    }
    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }

    private void _Decelerate(HeroHorizontalMovementsSettings settings)
    {
        _horizontalSpeed -= settings.deceleration * Time.fixedDeltaTime;
        if(_horizontalSpeed < 0)
        {
            _horizontalSpeed = 0;
        }
    }

    private void _UpdateHorizontalSpeed(HeroHorizontalMovementsSettings settings)
    {
        if (_moveDirX != 0)
        {
            _Accelerate(settings);
        } else
        {
            _Decelerate(settings);
        }
    }

    private HeroHorizontalMovementsSettings _GetCurrentHorizontalMovementsSettings()
    {
        if (IsTouchingGround)
        {
            return _groundHorizontalMovementsSettings;
        } else
        {
            return _airHorizontalMovementsSettings;
        }
    }
#endregion


    #region forces
    private void _ApplyFallGravity()
    {
        _verticalSpeed -= _fallSettings.fallGravity * Time.fixedDeltaTime;
        if (_verticalSpeed < -_fallSettings.fallSpeedMax)
        {
            _verticalSpeed = -_fallSettings.fallSpeedMax;
        }
    }

    private void _ApllyVerticalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.y = _verticalSpeed;
        _rigidbody.velocity = velocity;
    }

    private void _ChangeOrientFromHorizontalMovement()
    {
        if (_moveDirX == 0) return;
        _orientX = Mathf.Sign(_moveDirX);
    }

    private void _ApplyHorizontalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.x = _horizontalSpeed * _orientX;
        _rigidbody.velocity = velocity;
    }
    

    private void _UpdateOrientVisual()
    {
        
        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }

    private void _TurnBack(HeroHorizontalMovementsSettings settings)
    {
        _horizontalSpeed -= settings.turnBackFriction * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f)
        {
            _horizontalSpeed = 0f;
            _ChangeOrientFromHorizontalMovement();
        }
    }

    private bool _AreOrientAndMovementOpposite()
    {
        return _moveDirX * _orientX < 0f;
    }
    #endregion
    #region GUI
    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"MoveDirX = {_moveDirX}");
        GUILayout.Label($"OrientX = {_orientX}");
        if(IsTouchingGround)
        {
            GUILayout.Label("OnGround");
        } else {
             GUILayout.Label("InAir");
        }
        GUILayout.Label($"JumpState = {_jumpState}");
        GUILayout.Label($"Horizontal Speed = {_horizontalSpeed}");
        GUILayout.Label($"Vertical Speed = {_verticalSpeed}");
        GUILayout.EndVertical();


    }
    #endregion

}