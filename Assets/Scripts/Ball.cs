using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null) Debug.Log("RigidBody is null!");
    }

    public void Init(Vector3 velocity)
    {
        _rb.AddForce(velocity, ForceMode.Impulse);
    }
}
