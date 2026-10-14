using Unity.Burst.Intrinsics;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class CharacterMovement : MonoBehaviour
{
    // Component References
    private Rigidbody _rigidBody;
    private Animator _animator;

    // Inputs
    private float _horizontal;
    private float _vertical;
    private bool _runInput;

    // Movement
    public float _linearSpeed;
    private float _currentSpeed;
    public float _playerAcceleration;
    public float _rotationSpeed;

    // Animations
    private float _speedAnimation;
    private float _maxSpeedAnimation = 1.4f;

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        float dt = Time.deltaTime;

        Inputs();
        PlayerMovement(dt);
        PlayerRotation(dt);
        HandleAnimations();
    }

    private void Inputs()
    {
        _horizontal = Input.GetAxis("Horizontal");
        _vertical = Input.GetAxis("Vertical");
        _runInput = Input.GetKey(KeyCode.LeftShift);
    }

    // Linear Movement 
    private void PlayerMovement(float dt)
    {
        float targetSpeed = _vertical * _linearSpeed;
        float diffSpeed = targetSpeed - _currentSpeed;
        _currentSpeed += Mathf.Clamp(diffSpeed, -_playerAcceleration, _playerAcceleration);

        float runningMultiplier = 1;
        if (_runInput && _currentSpeed > 0)
        {
            runningMultiplier = 1.8f;
        }
        _speedAnimation = runningMultiplier;

        _rigidBody.MovePosition(transform.position + transform.forward * dt * _currentSpeed * runningMultiplier);
    }

    //Rotation
    private void PlayerRotation(float dt)
    {
        Vector3 rotation = _horizontal * transform.up * _rotationSpeed * dt;
        Quaternion deltaRotation = Quaternion.Euler(rotation);
        _rigidBody.MoveRotation(_rigidBody.rotation * deltaRotation);
    }

    // Animation Speed
    private void HandleAnimations()
    {
        _animator.SetFloat("Speed", _vertical * _speedAnimation);
        _speedAnimation = Mathf.Clamp( _speedAnimation, -_maxSpeedAnimation, _maxSpeedAnimation);
        _animator.speed = _speedAnimation;
    }
}
