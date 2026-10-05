using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField] private Ball _ballPrefab;
    [SerializeField] private Transform _ballsParent;
    [SerializeField] private SimulatedPhysics _simulatedPhysics;

    [Header("Rotation")]
    [SerializeField] private float _speed = 60f;                    // degrees per second
    [SerializeField] private Vector2 _yawLimits = new(-45f, 45f);   // A/D, Left/Right
    [SerializeField] private Vector2 _pitchLimits = new(-10f, 45f); // W/S, Up/Down

    [Header("Force")]
    [SerializeField] private float _force = 10f;
    [SerializeField] private float _forceChangeSpeed = 5f; //force units per second
    [SerializeField] private float _minForce = 1f;
    [SerializeField] private float _maxForce = 30f;

    private InputActions _input;
    private Quaternion _startRotation;
    private float _yaw;
    private float _pitch;

    private void Awake()
    {
        _input = new InputActions();
        _startRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        _input.Launcher.Enable();
        _input.Launcher.Shoot.performed += Shoot_performed;
    }

    private void OnDisable()
    {
        _input.Launcher.Shoot.performed -= Shoot_performed;
        _input.Launcher.Disable();
    }

    void Update()
    {
        HandleRotation();
        HandleForce();
        _simulatedPhysics.SimulateTrajectory(_ballPrefab, transform.position, transform.forward * _force);
    }

    private void HandleRotation()
    {
        Vector2 input = _input.Launcher.Rotation.ReadValue<Vector2>(); // read every frame

        _yaw += input.x * _speed * Time.deltaTime;
        _pitch += input.y * _speed * Time.deltaTime;

        _yaw = Mathf.Clamp(_yaw, _yawLimits.x, _yawLimits.y);
        _pitch = Mathf.Clamp(_pitch, _pitchLimits.x, _pitchLimits.y);

        transform.localRotation = _startRotation * Quaternion.Euler(-_pitch, _yaw, 0f);
    }

    private void HandleForce()
    {
        float input = _input.Launcher.Force.ReadValue<float>();   // Q = +1, E = -1, none = 0
        _force += input * _forceChangeSpeed * Time.deltaTime;
        _force = Mathf.Clamp(_force, _minForce, _maxForce);
    }

    private void Shoot_performed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        var _spawned = Instantiate(_ballPrefab, transform.position, transform.rotation, _ballsParent);
        _spawned.Init(transform.forward * _force);
    }
}
