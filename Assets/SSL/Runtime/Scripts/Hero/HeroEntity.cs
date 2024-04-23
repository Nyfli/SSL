using UnityEngine;

public class HeroEntity : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody2D _rigidbody;

    [Header("Horizontal Movements")]
    [SerializeField] private HeroHorizontalMovementsSettings _movementsSettings;
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

    [Header("Jump")]
    [SerializeField] private HeroJumpSettings _jumpSettings;
    [SerializeField] private HeroFallSettings _jumpFallSettings;


    enum JumpState
    {
        NotJumping,
        JumpingImpulsion,
        Falling,
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

    private void _UpdateJumpStateImpulsion()
    {
        _jumpTimer += Time.fixedDeltaTime;
        if (_jumpTimer < _jumpSettings.jumpMaxDuration)
        {
            _verticalSpeed = _jumpSettings.jumpSpeed;
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
        } else {
            _ResetVerticalSpeed();
            _jumpState = JumpState.NotJumping;
        }
    }

    private void _UpdateJump()
    {
        switch (_jumpState)
        {
            case JumpState.JumpingImpulsion:
                _UpdateJumpStateImpulsion();
                break;
            case JumpState.Falling:
                _UpdateJumpStateFalling();
                break;
        }
    }

    public void JumpStart()
    {
        _jumpState = JumpState.JumpingImpulsion;
        _jumpTimer = 0f;
    }

    public bool IsTouchingGround { get; private set; } = false;

    private void _ApplyGroundDetection()
    {
        IsTouchingGround = _groundDetector.DetectGroundNearBy();
    }

    private void _ResetVerticalSpeed()
    {
        _verticalSpeed = 0f;
    }

    private void _Accelerate()
    {
        _horizontalSpeed += _movementsSettings.acceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed > _movementsSettings.speedMax)
        {
            _horizontalSpeed = _movementsSettings.speedMax;
        }
    }

    private void _Decelerate()
    {
        _horizontalSpeed -= _movementsSettings.deceleration * Time.fixedDeltaTime;
        if(_horizontalSpeed < 0)
        {
            _horizontalSpeed = 0;
        }
    }

    private void _UpdateHorizontalSpeed()
    {
        if (_moveDirX != 0)
        {
            _Accelerate();
        } else
        {
            _Decelerate();
        }
    }

    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }

    private void FixedUpdate()
    {
        _ApplyGroundDetection();
        if (_AreOrientAndMovementOpposite())
        {
            _TurnBack();
        } else {
            _UpdateHorizontalSpeed();
            _ChangeOrientFromHorizontalMovement();
        }

        if (IsJumping)
        {
            _UpdateJump();
        } else
        {
            if (!IsTouchingGround)
            {
                _ApplyFallGravity(_fallSettings);
            }
            else
            {
                _ResetVerticalSpeed();
            }
        }

        _ApplyHorizontalSpeed();
        _ApllyVerticalSpeed();
    }

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
    
    private void Update()
    {
        _UpdateOrientVisual();
    }

    private void _UpdateOrientVisual()
    {
        
        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }

    private void _TurnBack()
    {
        _horizontalSpeed -= _movementsSettings.turnBackFriction * Time.fixedDeltaTime;
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
}