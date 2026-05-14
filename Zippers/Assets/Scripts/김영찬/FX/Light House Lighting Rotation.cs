using System;
using UnityEngine;
using UnityEngine.Serialization;

public class LightHouseLightingRotation : MonoBehaviour
{
    [Header("초당 회전할 각도 (Degree/sec)")]
    [SerializeField] float _rotationSpeed = 15f;
    
    private void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        transform.Rotate(Vector3.up * (_rotationSpeed * Time.deltaTime));
    }
}
