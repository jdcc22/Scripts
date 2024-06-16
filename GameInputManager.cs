using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputManager : MonoBehaviour
{
    bool isHoldingUp;
    PlayerInputActions inputActions;

    private void Awake()
    {
       inputActions = new PlayerInputActions();
       inputActions.Enable();
        inputActions.Punchtime.LeftPunch.performed += LeftPunch_performed;
        inputActions.Punchtime.RightPunch.performed += RightPunch_performed;
        inputActions.Punchtime.BlockModifier.started += BlockModifier_started;
        inputActions.Punchtime.BlockModifier.canceled += BlockModifier_canceled;
        inputActions.Punchtime.DodgeLeft.performed += DodgeLeft_performed;
        inputActions.Punchtime.DodgeRight.performed += DodgeRight_performed;
        inputActions.Punchtime.Duck.performed += Duck_performed;
        inputActions.Punchtime.StarPunch.performed += StarPunch_performed;
        inputActions.Punchtime.SingleStar.performed += SingleStar_performed;

        inputActions.Punchtime.Pause.performed += Pause_performed;
        inputActions.Punchtime.Candybar.performed += Candybar_performed;
    }

    public event EventHandler Candybar;
    private void Candybar_performed(InputAction.CallbackContext obj)
    {
        Candybar?.Invoke(this, null);
    }

    private void Pause_performed(InputAction.CallbackContext obj)
    {
        PausePerformed?.Invoke(this, null);
    }

    private void SingleStar_performed(InputAction.CallbackContext obj)
    {
        Action.Invoke(this, new actionContextEventArgs { bufferState = LittleMac.BufferStates.Single_Star });
    }

    private void StarPunch_performed(InputAction.CallbackContext obj)
    {
        Action.Invoke(this, new actionContextEventArgs { bufferState = LittleMac.BufferStates.Star_Punch });
    }

    private void Duck_performed(InputAction.CallbackContext obj)
    {
        Action.Invoke(this, new actionContextEventArgs { bufferState = LittleMac.BufferStates.Duck });
    }

    private void DodgeRight_performed(InputAction.CallbackContext obj)
    {
        Action.Invoke(this, new actionContextEventArgs { bufferState = LittleMac.BufferStates.Right_Dodge });
    }

    public class actionContextEventArgs : EventArgs
    {
        public LittleMac.BufferStates bufferState;
    }

    public class blockContextEventArgs : EventArgs
    {
        public bool isblocking;
    }

    private void DodgeLeft_performed(InputAction.CallbackContext obj)
    {
        Action.Invoke(this, new actionContextEventArgs { bufferState = LittleMac.BufferStates.Left_Dodge });
    }

    private void BlockModifier_canceled(InputAction.CallbackContext obj)
    {
        isHoldingUp = false;
        BlockState.Invoke(this, new blockContextEventArgs { isblocking = isHoldingUp });
    }

    private void BlockModifier_started(InputAction.CallbackContext obj)
    {
        isHoldingUp = true;
        BlockState.Invoke(this, new blockContextEventArgs { isblocking = isHoldingUp });
    }

    private void RightPunch_performed(InputAction.CallbackContext obj)
    {
        if (!isHoldingUp) Action.Invoke(this, new actionContextEventArgs { bufferState = LittleMac.BufferStates.Right_Hook });
        else Action.Invoke(this, new actionContextEventArgs { bufferState = LittleMac.BufferStates.Right_Jab });
    }

    private void LeftPunch_performed(InputAction.CallbackContext obj)
    {
      if (!isHoldingUp) Action.Invoke(this, new actionContextEventArgs { bufferState = LittleMac.BufferStates.Left_Hook });
      else Action.Invoke(this, new actionContextEventArgs { bufferState = LittleMac.BufferStates.Left_Jab });
    }

    public void Destroy()
    {
        inputActions.Punchtime.Disable();
    }

    public event EventHandler<actionContextEventArgs> Action;
    public event EventHandler<blockContextEventArgs> BlockState;

    public event EventHandler PausePerformed;
    

    
}
