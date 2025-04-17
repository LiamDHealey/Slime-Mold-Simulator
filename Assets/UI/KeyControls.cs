using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyControls : MonoBehaviour
{
    ComputeController computeController;

    private void Awake()
    {
        computeController = FindAnyObjectByType<ComputeController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (Input.GetKeyDown(KeyCode.Space))
            computeController.ResetSimulation();

        if (Input.GetKeyDown(KeyCode.H))
            foreach (Transform child in transform)
                child.gameObject.SetActive(!child.gameObject.activeSelf);

        if (Input.GetKeyDown(KeyCode.J))
            foreach (Transform child in computeController.transform)
                child.gameObject.SetActive(!child.gameObject.activeSelf);
    }
}
