using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMover : MonoBehaviour
{
    public float zoomSpeed = 1;
    public float rotationSpeed = 1;

    float distance;
    Vector3 lastMousePos;

    private void Start()
    {
        lastMousePos = Input.mousePosition;
        distance = transform.localPosition.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        distance = Mathf.Clamp(distance + Input.mouseScrollDelta.y * -zoomSpeed, 1.22474487f, 4f);

        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            var delta = Input.mousePosition - lastMousePos;
            delta = new(-delta.y, delta.x, delta.z);
            transform.localRotation *= Quaternion.Euler(delta * rotationSpeed);
        }

        transform.localPosition = transform.forward * -distance;


        lastMousePos = Input.mousePosition;
    }
}
