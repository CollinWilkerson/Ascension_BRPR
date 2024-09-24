using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Look Sensitivity")]
    public float sensX;
    public float sensY;

    [Header("Clamping")]
    public float minY;
    public float maxY;

    [Header("spectator")]
    public float spectatorMoveSpeed;

    private float rotX;
    private float rotY;

    private bool isSpectator;

    private void Start()
    {
        //this doesn't hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    //late update for cameras
    //the player is functionally just a camera
    private void LateUpdate()
    {
        rotX += Input.GetAxis("Mouse X") * sensX;
        rotY += Input.GetAxis("Mouse Y") * sensY;

        rotY = Mathf.Clamp(rotY, minY, maxY);
        
        if (isSpectator)
        {
            //changes spectator rotation based on view
            transform.rotation = Quaternion.Euler(-rotY, rotX, 0);

            //handles 3 axis movement with q and e being the verticle inputs
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            float y = 0;

            if (Input.GetKey(KeyCode.E))
            {
                y = 1;
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                y = -1;
            }
            //adjusts the xyz to the axis of rotation
            Vector3 dir = transform.right * x + transform.up * y + transform.forward * x;
            //adds the movement to the position
            transform.position += dir * spectatorMoveSpeed * Time.deltaTime;
        }
        else
        {
            //vertical rotation
            transform.localRotation = Quaternion.Euler(-rotY, 0, 0);

            //horizontal rotation that maintains vertical
            transform.parent.localRotation = Quaternion.Euler(0, rotX, 0);
        }
    }

    public void SetAsSpectator()
    {
        isSpectator = true;
        transform.parent = null;
    }

}
