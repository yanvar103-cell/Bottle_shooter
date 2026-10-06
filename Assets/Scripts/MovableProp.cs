using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// It has no hit reaction; it only registers a ghost that follows the prop for the trajectory preview.
[RequireComponent(typeof(Rigidbody))]
public class MovableProp : MonoBehaviour
{
    private void Start()
    {
        SimulatedPhysics.Instance.AddDynamicGhost(gameObject);
    }
}
