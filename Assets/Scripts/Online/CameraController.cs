using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.EventSystems;

public class CameraController : NetworkBehaviour
{
    public GameObject cameraHolder;
    //public Vector3 offset;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public override void OnStartAuthority()
    {
        cameraHolder.SetActive(true);
    }

    public void Update()
    {
        if (!isLocalPlayer) { return; }

        if (SceneManager.GetActiveScene().name == "OnlineGame")
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;

            //Cursor.visible = false;
            //Cursor.lockState = CursorLockMode.Locked;

            //cameraHolder.transform.position = transform.position;
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            cameraHolder.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
}
