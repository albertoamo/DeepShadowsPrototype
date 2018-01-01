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
    protected VKShadowController charShadowController;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // Initialization player variables
        Cursor.visible = false;

        charCamera = FindObjectOfType<VKThirdPersonCamera>();
        charController = GetComponent<VKThirdPersonController>();
        charShadowController = FindObjectOfType<VKShadowController>();

        if (charController) charController.Init();
        if (charCamera) charCamera.SetTarget(this.gameObject);
        if (charShadowController) charShadowController.Init();
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
        charController.UpdatePlayerStatus(charShadowController.target, charShadowController.IsPlayerInShadowsLed());
    }

    protected virtual void LateUpdate()
    {
        if (charController == null) return;	  
        
        InputHandle();
    }

    protected void InputHandle()
    {
        // Camera movement
        charCamera.input.x = Input.GetAxis(rotateCameraXInput);
        charCamera.input.y = Input.GetAxis(rotateCameraYInput);

        // Character movement
        charController.input.x = Input.GetAxis(horizontalInput);
        charController.input.y = Input.GetAxis(verticallInput);
        charController.lookDirection = charCamera.GetCameraDirection(charController.input);
       
        #region InputKeys
        // just a example to quit the application 
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Cursor.visible) Cursor.visible = true;
            else Application.Quit();
        }

        if (Input.GetKeyDown(sprintInput))
        {
            //charController.Sprint(true);
        }

        if (Input.GetKeyDown(jumpInput))
        {
            //charController.Jump();
        }

        // ShadowDiving related input
        if (Input.GetMouseButtonDown(0) && charShadowController.isShadow == true) // Change status
        {
            // Fix this in the future
            if(charController.ExitShadowMode(charShadowController.target))
            {

            }
            else if(charController.EnterShadowMode(charShadowController.target))
            {

            }
        }

        #endregion
    }
}
