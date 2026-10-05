using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bottle : MonoBehaviour
{
    [SerializeField] private float _fallAngle = 45f;       // tilted more than this = knocked over
    [SerializeField] private float _fallDistance = 0.3f;   // dropped this far below its start height = knocked off
    [SerializeField] private float _disappearDelay = 2f;   // seconds after being knocked before it disappears

    private bool _isShot;
    private float _startHeight;
    private GameObject _ghost;   // invisible copy in the simulated scene

    private void Start()
    {
        _startHeight = transform.position.y;
    }

    public void SetGhost(GameObject ghost)
    {
        _ghost = ghost;
    }

    private void FixedUpdate()
    {
        if (_isShot) return;

        bool tippedOver = Vector3.Angle(transform.up, Vector3.up) > _fallAngle;
        bool fellDown = transform.position.y < _startHeight - _fallDistance;

        if (tippedOver || fellDown)
            MarkAsShot();
    }

    private void MarkAsShot()
    {
        _isShot = true;

        //remove the ghost so the trajectory line no longer bounces off this bottle
        if (_ghost != null) Destroy(_ghost);

        if (GameManager.Instance != null)
            GameManager.Instance.BottleShot();

        Destroy(gameObject, _disappearDelay);
    }
}
