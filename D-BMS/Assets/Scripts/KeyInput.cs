using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Runtime.InteropServices;
using System.Threading;
using System;

public class KeyInput : MonoBehaviour
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
    private static extern int GetAsyncKeyState(int keyCode);
    public bool[] prevKeyState;

    bool isAssistKeyUse;
    private int[] keyCodeArray;

    private InputAction funcGamePauseAction;
    private InputAction funcGameRestartAction;
    private InputAction funcSpeedUpAction;
    private InputAction funcSpeedDownAction;
    private InputAction funcSpeedUp2Action;
    private InputAction funcSpeedDown2Action;
    private InputAction funcJudgeAdjUpAction;
    private InputAction funcJudgeAdjDownAction;

    [SerializeField]
    private BMSGameManager bmsGameManager;

    private Thread inputThread;
    private TimeSpan threadFrequency;

    void Awake()
    {
        prevKeyState = new bool[6];
        for (int i = 0; i < 6; i++)
        {
            prevKeyState[i] = true;
        }

        isAssistKeyUse = PlayerPrefs.GetInt("AssistKeyUse") == 1 ? true : false;
        keyCodeArray = new int[6];
        for (int i = 0; i < 6; i++)
        {
            keyCodeArray[i] = PlayerPrefs.GetInt($"Key{i + 1}");
        }

        threadFrequency = new TimeSpan(10000000 / PlayerPrefs.GetInt("PollingRate"));

        funcGamePauseAction = new InputAction("FuncGamePause", InputActionType.Button, "<Keyboard>/Escape");
        funcGameRestartAction = new InputAction("FuncGameRestart", InputActionType.Button, "<Keyboard>/F5");
        funcSpeedUpAction = new InputAction("FuncSpeedUp", InputActionType.Button, $"<Keyboard>/{KeyCodeToString(PlayerPrefs.GetInt("SpeedUp1"))}");
        funcSpeedDownAction = new InputAction("FuncSpeedDown", InputActionType.Button, $"<Keyboard>/{KeyCodeToString(PlayerPrefs.GetInt("SpeedDown1"))}");
        funcSpeedUp2Action = new InputAction("FuncSpeedUp2", InputActionType.Button, $"<Keyboard>/{KeyCodeToString(PlayerPrefs.GetInt("SpeedUp2"))}");
        funcSpeedDown2Action = new InputAction("FuncSpeedDown2", InputActionType.Button, $"<Keyboard>/{KeyCodeToString(PlayerPrefs.GetInt("SpeedDown2"))}");
        funcJudgeAdjUpAction = new InputAction("FuncJudgeAdjUp", InputActionType.Button, "<Keyboard>/F8");
        funcJudgeAdjDownAction = new InputAction("FuncJudgeAdjDown", InputActionType.Button, "<Keyboard>/F7");
    }

    public void InputThreadStart()
    {
        if (isAssistKeyUse)
        {
            inputThread = new Thread(KeyInputThread_Assist);
        }
        else
        {
            inputThread = new Thread(KeyInputThread_NotAssist);
        }
        
        inputThread.Start();
    }

    private void KeyInputThread_NotAssist()
    {
        while (true)
        {
            #region Key Input Check
            int result = KeyState(0);
            switch (result)
            {
                case 1:
                    bmsGameManager.KeyDown(0);
                    break;
                case 2:
                    bmsGameManager.KeyUp(0);
                    break;
            }

            result = KeyState(1);
            switch (result)
            {
                case 1:
                    bmsGameManager.KeyDown(1);
                    break;
                case 2:
                    bmsGameManager.KeyUp(1);
                    break;
            }

            result = KeyState(2);
            switch (result)
            {
                case 1:
                    bmsGameManager.KeyDown(2);
                    break;
                case 2:
                    bmsGameManager.KeyUp(2);
                    break;
            }

            result = KeyState(3);
            switch (result)
            {
                case 1:
                    bmsGameManager.KeyDown(3);
                    break;
                case 2:
                    bmsGameManager.KeyUp(3);
                    break;
            }

            result = KeyState(4);
            switch (result)
            {
                case 1:
                    bmsGameManager.KeyDown(4);
                    break;
                case 2:
                    bmsGameManager.KeyUp(4);
                    break;
            }
            #endregion

            Thread.Sleep(threadFrequency);
        }
    }

    private void KeyInputThread_Assist()
    {
        while (true)
        {
            #region Key Input Check
            int result = KeyState(0);
            switch (result)
            {
                case 1:
                    bmsGameManager.KeyDown(0);
                    break;
                case 2:
                    bmsGameManager.KeyUp(0);
                    break;
            }

            result = KeyState(1);
            switch (result)
            {
                case 1:
                    bmsGameManager.KeyDown(1);
                    break;
                case 2:
                    bmsGameManager.KeyUp(1);
                    break;
            }

            result = KeyState_Assist(2);
            switch (result)
            {
                case 1:
                    bmsGameManager.KeyDown(2);
                    break;
                case 2:
                    bmsGameManager.KeyUp(2);
                    break;
            }

            result = KeyState_Assist(5);
            switch (result)
            {
                case 1:
                    bmsGameManager.KeyDown(2);
                    break;
                case 2:
                    bmsGameManager.KeyUp(2);
                    break;
            }

            result = KeyState(3);
            switch (result)
            {
                case 1:
                    bmsGameManager.KeyDown(3);
                    break;
                case 2:
                    bmsGameManager.KeyUp(3);
                    break;
            }

            result = KeyState(4);
            switch (result)
            {
                case 1:
                    bmsGameManager.KeyDown(4);
                    break;
                case 2:
                    bmsGameManager.KeyUp(4);
                    break;
            }
            #endregion

            Thread.Sleep(threadFrequency);
        }
    }

    private int KeyState(int index)
    {
        int result = 0;
        int state = GetAsyncKeyState(keyCodeArray[index]);
        if (state == 0 || state == 1)
        {
            if (prevKeyState[index])
            {
                result = 2;
                prevKeyState[index] = false;
            }
        }
        else
        {
            if (!prevKeyState[index])
            {
                result = 1;
                prevKeyState[index] = true;
            }
        }
        return result;
    }

    private int KeyState_Assist(int index)
    {
        if (prevKeyState[index == 2 ? 5 : 2]) { return 0; }

        int result = 0;
        int state = GetAsyncKeyState(keyCodeArray[index]);
        if (state == 0 || state == 1)
        {
            if (prevKeyState[index])
            {
                result = 2;
                prevKeyState[index] = false;
            }
        }
        else
        {
            if (!prevKeyState[index])
            {
                result = 1;
                prevKeyState[index] = true;
            }
        }
        return result;
    }

    private string KeyCodeToString(int key)
    {
        string value = "";
        switch (key)
        {
            case 0x0D: value = "Enter"; break;
            case 0x20: value = "Space"; break;
            case 0x21: value = "PageUp"; break;
            case 0x22: value = "PageDown"; break;
            case 0x23: value = "End"; break;
            case 0x24: value = "Home"; break;
            case 0x25: value = "LeftArrow"; break;
            case 0x26: value = "UpArrow"; break;
            case 0x27: value = "RightArrow"; break;
            case 0x28: value = "DownArrow"; break;
            case 0x2D: value = "Insert"; break;
            case 0x2E: value = "Delete"; break;
            case 0x30: value = "0"; break;
            case 0x31: value = "1"; break;
            case 0x32: value = "2"; break;
            case 0x33: value = "3"; break;
            case 0x34: value = "4"; break;
            case 0x35: value = "5"; break;
            case 0x36: value = "6"; break;
            case 0x37: value = "7"; break;
            case 0x38: value = "8"; break;
            case 0x39: value = "9"; break;
            case 0x41: value = "A"; break;
            case 0x42: value = "B"; break;
            case 0x43: value = "C"; break;
            case 0x44: value = "D"; break;
            case 0x45: value = "E"; break;
            case 0x46: value = "F"; break;
            case 0x47: value = "G"; break;
            case 0x48: value = "H"; break;
            case 0x49: value = "I"; break;
            case 0x4A: value = "J"; break;
            case 0x4B: value = "K"; break;
            case 0x4C: value = "L"; break;
            case 0x4D: value = "M"; break;
            case 0x4E: value = "N"; break;
            case 0x4F: value = "O"; break;
            case 0x50: value = "P"; break;
            case 0x51: value = "Q"; break;
            case 0x52: value = "R"; break;
            case 0x53: value = "S"; break;
            case 0x54: value = "T"; break;
            case 0x55: value = "U"; break;
            case 0x56: value = "V"; break;
            case 0x57: value = "W"; break;
            case 0x58: value = "X"; break;
            case 0x59: value = "Y"; break;
            case 0x5A: value = "Z"; break;
            case 0x60: value = "NumPad0"; break;
            case 0x61: value = "NumPad1"; break;
            case 0x62: value = "NumPad2"; break;
            case 0x63: value = "NumPad3"; break;
            case 0x64: value = "NumPad4"; break;
            case 0x65: value = "NumPad5"; break;
            case 0x66: value = "NumPad6"; break;
            case 0x67: value = "NumPad7"; break;
            case 0x68: value = "NumPad8"; break;
            case 0x69: value = "NumPad9"; break;
            case 0x6A: value = "NumPadMultiply"; break;
            case 0x6B: value = "NumpadPlus"; break;
            case 0x6C: value = "NumpadEnter"; break;
            case 0x6D: value = "NumpadMinus"; break;
            case 0x6E: value = "NumpadPeriod"; break;
            case 0x6F: value = "NumpadDivide"; break;
            case 0x70: value = "F1"; break;
            case 0x71: value = "F2"; break;
            case 0x72: value = "F3"; break;
            case 0x73: value = "F4"; break;
            case 0x75: value = "F6"; break;
            case 0x78: value = "F9"; break;
            case 0x79: value = "F10"; break;
            case 0x7A: value = "F11"; break;
            case 0x7B: value = "F12"; break;
            case 0xA0: value = "LeftShift"; break;
            case 0xA1: value = "RightShift"; break;
            case 0xA2: value = "LeftCtrl"; break;
            case 0xA3: value = "RightCtrl"; break;
            case 0xA4: value = "LeftAlt"; break;
            case 0xA5: value = "RightAlt"; break;
            case 0xBA: value = "Semicolon"; break;
            case 0xBB: value = "Equals"; break;
            case 0xBC: value = "Comma"; break;
            case 0xBD: value = "Minus"; break;
            case 0xBE: value = "Period"; break;
            case 0xBF: value = "Slash"; break;
            case 0xC0: value = "Backquote"; break;
            case 0xDB: value = "LeftBracket"; break;
            case 0xDC: value = "Backslash"; break;
            case 0xDD: value = "RightBracket"; break;
            case 0xDE: value = "Quote"; break;
        }
        return value;
    }

    public void KeySetting()
    {
        MakeFunctionKeyAction();
    }

    public void KeyDisable()
    {
        DeleteFunctionKeyAction();
    }

    public void InputThreadAbort()
    {
        if (inputThread != null && inputThread.IsAlive)
        {
            inputThread.Abort();
        }
    }

    private void MakeFunctionKeyAction()
    {
        funcGamePauseAction.started += ctx => { bmsGameManager.GamePause(0); };
        funcGameRestartAction.started += ctx => { StartCoroutine(bmsGameManager.GameRestart()); };
        funcSpeedUpAction.started += ctx => { bmsGameManager.ChangeSpeed(1); };
        funcSpeedDownAction.started += ctx => { bmsGameManager.ChangeSpeed(-1); };
        funcSpeedUp2Action.started += ctx => { bmsGameManager.ChangeSpeed(PlayerPrefs.GetInt("NoteSpeed")); };
        funcSpeedDown2Action.started += ctx => { bmsGameManager.ChangeSpeed(-(int)(PlayerPrefs.GetInt("NoteSpeed") * 0.5f)); };
        funcJudgeAdjUpAction.started += ctx => { bmsGameManager.ChangeJudgeAdjValue(1); };
        funcJudgeAdjDownAction.started += ctx => { bmsGameManager.ChangeJudgeAdjValue(-1); };

        funcGamePauseAction.Enable();
        funcGameRestartAction.Enable();
        funcSpeedUpAction.Enable();
        funcSpeedDownAction.Enable();
        funcSpeedUp2Action.Enable();
        funcSpeedDown2Action.Enable();
        funcJudgeAdjUpAction.Enable();
        funcJudgeAdjDownAction.Enable();
    }

    private void DeleteFunctionKeyAction()
    {
        if (funcGamePauseAction == null) { return; }

        funcGamePauseAction.started -= ctx => { bmsGameManager.GamePause(0); };
        funcGameRestartAction.started -= ctx => { StartCoroutine(bmsGameManager.GameRestart()); };
        funcSpeedUpAction.started -= ctx => { bmsGameManager.ChangeSpeed(1); };
        funcSpeedDownAction.started -= ctx => { bmsGameManager.ChangeSpeed(-1); };
        funcSpeedUp2Action.started -= ctx => { bmsGameManager.ChangeSpeed(PlayerPrefs.GetInt("NoteSpeed")); };
        funcSpeedDown2Action.started -= ctx => { bmsGameManager.ChangeSpeed(-(int)(PlayerPrefs.GetInt("NoteSpeed") * 0.5f)); };
        funcJudgeAdjUpAction.started -= ctx => { bmsGameManager.ChangeJudgeAdjValue(1); };
        funcJudgeAdjDownAction.started -= ctx => { bmsGameManager.ChangeJudgeAdjValue(-1); };

        funcGamePauseAction.Disable();
        funcGameRestartAction.Disable();
        funcSpeedUpAction.Disable();
        funcSpeedDownAction.Disable();
        funcSpeedUp2Action.Disable();
        funcSpeedDown2Action.Disable();
        funcJudgeAdjUpAction.Disable();
        funcJudgeAdjDownAction.Disable();
        funcGamePauseAction = null;
        funcGameRestartAction = null;
        funcSpeedUpAction = null;
        funcSpeedDownAction = null;
        funcSpeedUp2Action = null;
        funcSpeedDown2Action = null;
        funcJudgeAdjUpAction = null;
        funcJudgeAdjDownAction = null;
    }
}
