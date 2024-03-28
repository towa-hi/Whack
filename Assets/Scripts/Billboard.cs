using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Camera mainCamera;
    float initialZRotation;

    void Start()
    {
        mainCamera = Camera.main;
        // Store the initial local Z rotation
        initialZRotation = transform.localEulerAngles.z;
    }

    void LateUpdate()
    {
        // Make the object face the camera by aligning it with the camera's rotation
        // This aligns the forward direction of the object with the forward direction of the camera
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);

        // Preserve the initial Z rotation by applying it on top of the billboard effect
        // Note: If you dynamically change the Z rotation (e.g., spinning the object),
        // you should update initialZRotation accordingly
        Vector3 currentEulerAngles = transform.eulerAngles;
        transform.eulerAngles = new Vector3(currentEulerAngles.x, currentEulerAngles.y, initialZRotation);
    }
}
