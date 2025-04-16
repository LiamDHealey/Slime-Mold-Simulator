using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CameraMover : MonoBehaviour
{
    public float zoomSpeed = 1;
    public float rotationSpeed = 1;

    float distance;
    private float right;
    Vector3 lastMousePos;

    private void Start()
    {
        lastMousePos = Input.mousePosition;
        distance = transform.localPosition.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        //distance = Mathf.Clamp(distance + Input.mouseScrollDelta.y * -zoomSpeed, -5, 4f);

        if (Input.GetKey(KeyCode.W))
        {
            distance = Mathf.Clamp(distance - zoomSpeed, -5, 4f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            distance = Mathf.Clamp(distance + zoomSpeed, -5, 4f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            right = Mathf.Clamp(right - zoomSpeed, -5, 5f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            right = Mathf.Clamp(right + zoomSpeed, -5, 5f);
        }

        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            var delta = Input.mousePosition - lastMousePos;
            delta = new(-delta.y, delta.x, delta.z);
            transform.localRotation *= Quaternion.Euler(delta * rotationSpeed);
        }

        transform.localPosition = transform.forward * -distance + transform.right * right;


        lastMousePos = Input.mousePosition;
    }
}
