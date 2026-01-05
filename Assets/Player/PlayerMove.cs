using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _topSpeed = 40f;
    [SerializeField] private float _accelerationRate = 10f;
    [SerializeField] private float _decelerationRate = 15f;
    [SerializeField] private float _turnSpeed = 8f;
    
    [Header("Steering Settings")]
    [SerializeField] private float _steeringSpeed = 2f;

    [Header("Gravity")]
    [SerializeField] private float _gravity = 50f;
    [SerializeField] private float _groundedGravity = -10f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private KeyCode _jumpKey = KeyCode.Space;

    [Header("References")]
    [SerializeField] private Transform _cameraTransform;

    private CharacterController _controller;

    private Vector3 _moveVector;
    private Vector3 _inputDirection;
    private Vector3 _currentMoveDirection;
    private Vector3 _velocity;
    
    private bool _isMoving = false;
    private Vector3 _lastMoveDirection;
    private float _verticalVelocity = 0f;

    public float Speed => new Vector3(_velocity.x, 0, _velocity.z).magnitude;
    public bool IsGrounded => _controller.isGrounded;
    
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        if (_cameraTransform == null && Camera.main != null)
            _cameraTransform = Camera.main.transform;
        
        _currentMoveDirection = transform.forward;
        _lastMoveDirection = transform.forward;
    }

    private void Update()
    {
        ReadInput();
        CalculateInputDirection();
        UpdateMovementState();
        HandleJumpInput();
        ApplyMovement();
    }

    private void ReadInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;

        _moveVector = new Vector3(horizontal, 0, vertical).normalized;
    }

    private void CalculateInputDirection()
    {
        if (_moveVector.magnitude < 0.1f)
        {
            _inputDirection = Vector3.zero;
            return;
        }

        if (_cameraTransform != null)
        {
            Vector3 camForward = _cameraTransform.forward;
            Vector3 camRight = _cameraTransform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            _inputDirection = (camForward * _moveVector.z + camRight * _moveVector.x).normalized;
        }
        else
        {
            _inputDirection = _moveVector.normalized;
        }
    }

    private bool IsOppositeDirection()
    {
        if (_inputDirection.magnitude < 0.1f || _lastMoveDirection.magnitude < 0.1f)
            return false;

        float dot = Vector3.Dot(_inputDirection.normalized, _lastMoveDirection.normalized);
        return dot < -0.5f;
    }

    private void UpdateMovementState()
    {
        if (_inputDirection.magnitude > 0.1f)
        {
            if (_isMoving && IsOppositeDirection())
            {
                _isMoving = false;
            }
            else
            {
                _isMoving = true;
                _lastMoveDirection = _inputDirection;
            }
        }
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(_jumpKey) && _controller.isGrounded)
        {
            _verticalVelocity = _jumpForce;
        }
    }

    private void ApplyMovement()
    {
        float deltaTime = Time.deltaTime;
        
        Vector3 planarVelocity = new Vector3(_velocity.x, 0, _velocity.z);
        float currentSpeed = planarVelocity.magnitude;

        if (_isMoving)
        {
            Vector3 cameraForward = _cameraTransform.forward;
            Vector3 cameraRight = _cameraTransform.right;
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 targetDirection;
            if (_inputDirection.magnitude > 0.1f)
            {
                targetDirection = _inputDirection;
            }
            else
            {
                targetDirection = cameraForward;
            }

            if (currentSpeed > 0.5f)
            {
                _currentMoveDirection = Vector3.Slerp(
                    _currentMoveDirection,
                    targetDirection,
                    _steeringSpeed * deltaTime
                );
            }
            else
            {
                _currentMoveDirection = targetDirection;
            }
            
            if (currentSpeed < _topSpeed)
            {
                planarVelocity += _currentMoveDirection * (_accelerationRate * deltaTime);
            }
            
            if (currentSpeed > 0.1f)
            {
                Vector3 targetVelocity = _currentMoveDirection * currentSpeed;
                planarVelocity = Vector3.Slerp(planarVelocity, targetVelocity, 
                    _turnSpeed * deltaTime);
            }
        }
        else
        {
            if (currentSpeed > 0.02f)
            {
                planarVelocity = Vector3.MoveTowards(planarVelocity, Vector3.zero,
                    _decelerationRate * deltaTime);
                    
                if (planarVelocity.magnitude > 0.1f)
                {
                    _currentMoveDirection = planarVelocity.normalized;
                }
            }
            else
            {
                planarVelocity = Vector3.zero;
            }
        }

        if (planarVelocity.magnitude > _topSpeed)
        {
            planarVelocity = planarVelocity.normalized * _topSpeed;
        }

        if (_controller.isGrounded && _verticalVelocity <= 0)
        {
            _verticalVelocity = _groundedGravity;
        }
        else
        {
            _verticalVelocity -= _gravity * deltaTime;
        }

        _velocity = new Vector3(planarVelocity.x, _verticalVelocity, planarVelocity.z);
        _controller.Move(_velocity * deltaTime);

        if (currentSpeed > 0.5f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_currentMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                _turnSpeed * deltaTime);
        }
    }
}