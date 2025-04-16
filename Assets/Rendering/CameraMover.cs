using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CameraMover : MonoBehaviour
{
    public float zoomSpeed = 1f;
    public float moveSpeed = 1;
    public float rotationSpeed = 1;

    float distance;
    Vector3 lastMousePos;

    private void Start()
    {
        lastMousePos = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && !EventSystem.current.IsPointerOverGameObject())
        {
            var delta = Input.mousePosition - lastMousePos;
            delta = new(-delta.y, delta.x, delta.z);
            transform.localRotation *= Quaternion.Euler(delta * rotationSpeed);
        }
        lastMousePos = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            distance = transform.localPosition.magnitude;
            transform.rotation = Quaternion.LookRotation(-transform.localPosition.normalized, transform.up);
        }
        if (Input.GetMouseButton(0))
        {
            distance = Mathf.Clamp(distance + Input.mouseScrollDelta.y * -zoomSpeed, -5, 4f);
            transform.localPosition = -transform.forward * distance;
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.localPosition += transform.forward * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.localPosition += -transform.forward * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.localPosition += -transform.right * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.localPosition += transform.right * moveSpeed * Time.deltaTime;
            }
        }
    }
}
