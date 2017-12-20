using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VKThirdPersonInput : MonoBehaviour {

    #region variables

    [Header("Default Inputs")]
    public string horizontalInput = "Horizontal";
    public string verticallInput = "Vertical";
    public KeyCode jumpInput = KeyCode.Space;
    public KeyCode crouchInput = KeyCode.Space;
    public KeyCode sprintInput = KeyCode.LeftShift;

    [Header("Camera Settings")]
    public string rotateCameraXInput = "Mouse X";
    public string rotateCameraYInput = "Mouse Y";

    [HideInInspector] public string customCameraState; 
    [HideInInspector] public string customlookAtPoint;    
    [HideInInspector] public bool changeCameraState;      
    [HideInInspector] public bool smoothCameraState; 
    [HideInInspector] public bool keepDirection;

    protected VKThirdPersonCamera charCamera;
    protected VKThirdPersonController charController;            

    #endregion

    // Use this for initialization
    void Start ()
    {
        charController = GetComponent<VKThirdPersonController>();
        if (charController != null)
            charController.Init();

        charCamera = FindObjectOfType<VKThirdPersonCamera>();
        //if (charCamera) charCamera.SetMainTarget(this.transform);
    }
	
	// Update is called once per frame
	protected virtual void Update ()
    {
        charController.UpdateController();
        charController.UpdateAnimator();
    }

    protected virtual void FixedUpdate()
    {
        // TO-DO
    }

    protected virtual void LateUpdate()
    {
        if (charController == null) return;	    
        InputHandle(); 
    }

    protected void InputHandle()
    {
        // Camera movement
        charCamera.input.x = Input.GetAxis(rotateCameraYInput);
        charCamera.input.y = Input.GetAxis(rotateCameraXInput);

        // Character movement
        charController.input.x = Input.GetAxis(horizontalInput);
        charController.input.y = Input.GetAxis(verticallInput);

        #region InputKeys
        // just a example to quit the application 
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Cursor.visible)
                Cursor.visible = true;
            else
                Application.Quit();
        }

        if (Input.GetKeyDown(sprintInput))
        {
            //charController.Sprint(true);
        }

        if (Input.GetKeyDown(jumpInput))
        {
            //charController.Jump();
        }
        #endregion
    }
}
