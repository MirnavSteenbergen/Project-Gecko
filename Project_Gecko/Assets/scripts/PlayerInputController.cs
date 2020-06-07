using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [Header("Input")]
    public bool useTouchControls = false;

    [Header("Touch Controls")]
    public FixedJoystick joystick;

    Player player;
    PlayerInput playerInput;

    private void Awake()
    {
        player = GetComponent<Player>();
        playerInput = GetComponent<PlayerInput>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        TouchMoveUpdate();

        if (Input.GetKeyDown(KeyCode.C))
        {
            playerInput.SwitchCurrentControlScheme("Gamepad");
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            playerInput.SwitchCurrentControlScheme("KeyBoard");
        }
    }

    #region Touch Input

    public void LetGoButtonPressed()
    {
        playerInput.SwitchCurrentControlScheme("Touch");

        player.inputLetGo = true;
    }

    public void LetGoButtonReleased()
    {
        playerInput.SwitchCurrentControlScheme("Touch");

        player.inputLetGo = false;
    }

    void TouchMoveUpdate()
    {
        float hor = joystick.Horizontal;
        float ver = joystick.Vertical;

        if (hor != 0 || ver != 0)
        {
            playerInput.SwitchCurrentControlScheme("Touch");
        }

        if (hor > 0) hor = 1;
        if (hor < 0) hor = -1;
        if (ver > 0) ver = 1;
        if (ver < 0) ver = -1;

        if (playerInput.currentControlScheme == "Touch")
        {
            player.inputMove = new Vector2(hor, ver);
        }
    }

    #endregion

    #region Action Map Input

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!useTouchControls)
        {
            player.inputMove = context.ReadValue<Vector2>();
        }
    }

    public void OnLetGo(InputAction.CallbackContext context)
    {
        if (context.started) player.inputLetGo = true;
        if (context.canceled) player.inputLetGo = false;
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.started) player.inputLetGo = false;
        if (context.canceled) player.inputLetGo = true;
    }

    #endregion

    public void OnDeviceChanged()
    {
        Debug.Log(playerInput.currentControlScheme);
    }
}
