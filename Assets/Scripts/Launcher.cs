using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField] private Ball _ballPrefab;
    [SerializeField] private float _ballLifetime = 5f;
    [SerializeField] private Transform _ballsParent;
    [SerializeField] private SimulatedPhysics _simulatedPhysics;

    [Header("Rotation")]
    [SerializeField] private float _speed = 40f;                    // degrees per second
    [SerializeField] private float _keyboardAcceleration = 4f;      // how fast a held key ramps up to full speed
    [SerializeField] private float _mouseSensitivity = 0.08f;       // mouse: degrees per pixel moved
    [SerializeField] private float _precisionMultiplier = 0.25f;    // speed while holding Shift
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
    private Vector2 _smoothedKeyInput;

    private void Awake()
    {
        _input = new InputActions();
        _startRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        _input.Launcher.Enable();
        _input.Launcher.Shoot.performed += Shoot_performed;

        //hide and lock the cursor while aiming (in a browser this applies after the player clicks the game)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        _input.Launcher.Shoot.performed -= Shoot_performed;
        _input.Launcher.Disable();
        
        //free the cursor during the intro and on the end screen, so the Restart button can be clicked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        _input.Dispose();
    }

    void Update()
    {
        HandleRotation();
        HandleForce();
        _simulatedPhysics.SimulateTrajectory(_ballPrefab, transform.position, transform.forward * _force);
    }

    private void HandleRotation()
    {
        float precision = _input.Launcher.Precision.IsPressed() ? _precisionMultiplier : 1f;

        //Keyboard: ramp the input up instead of jumping to full speed,
        //so a short tap moves only a tiny bit and holding the key speeds up
        Vector2 keyInput = _input.Launcher.Rotation.ReadValue<Vector2>();
        _smoothedKeyInput = Vector2.MoveTowards(_smoothedKeyInput, keyInput, _keyboardAcceleration * Time.deltaTime);
        Vector2 delta = _smoothedKeyInput * _speed * precision * Time.deltaTime;

        //Mouse: delta is already "pixels moved this frame", so no Time.deltaTime here
        Vector2 mouseDelta = _input.Launcher.Look.ReadValue<Vector2>();
        delta += mouseDelta * _mouseSensitivity * precision;

        _yaw = Mathf.Clamp(_yaw + delta.x, _yawLimits.x, _yawLimits.y);
        _pitch = Mathf.Clamp(_pitch + delta.y, _pitchLimits.x, _pitchLimits.y);

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
        Destroy(_spawned.gameObject, _ballLifetime);
    }
}
