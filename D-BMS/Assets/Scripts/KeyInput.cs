using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class KeyInput : MonoBehaviour
{
    bool isAssistKeyUse;
    // 1번키부터 5번키까지 입력액션
    private InputAction keyInputAction1;
    private InputAction keyInputAction2;
    private InputAction keyInputAction3;
    private InputAction keyInputAction4;
    private InputAction keyInputAction5;
    private InputAction keyInputAction6;  // 3번 보조키

    private InputAction funcGameEndAction;
    private InputAction funcGameRestartAction;
    private InputAction funcSpeedUpAction;
    private InputAction funcSpeedDownAction;
    private InputAction funcSpeedUp2Action;
    private InputAction funcSpeedDown2Action;
    private InputAction funcJudgeAdjUpAction;
    private InputAction funcJudgeAdjDownAction;

    [SerializeField]
    private BMSGameManager bmsGameManager;

    void Awake()
    {
        isAssistKeyUse = PlayerPrefs.GetInt("AssistKeyUse") == 1 ? true : false;
        keyInputAction1 = new InputAction("KeyInput1", InputActionType.Button, $"<Keyboard>/{PlayerPrefs.GetString("Key1")}");
        keyInputAction2 = new InputAction("KeyInput2", InputActionType.Button, $"<Keyboard>/{PlayerPrefs.GetString("Key2")}");
        keyInputAction3 = new InputAction("KeyInput3", InputActionType.Button, $"<Keyboard>/{PlayerPrefs.GetString("Key3")}");
        keyInputAction4 = new InputAction("KeyInput4", InputActionType.Button, $"<Keyboard>/{PlayerPrefs.GetString("Key4")}");
        keyInputAction5 = new InputAction("KeyInput5", InputActionType.Button, $"<Keyboard>/{PlayerPrefs.GetString("Key5")}");
        if (isAssistKeyUse) { keyInputAction6 = new InputAction("KeyInput6", InputActionType.Button, $"<Keyboard>/{PlayerPrefs.GetString("Key6")}"); }

        funcGameEndAction = new InputAction("FuncGameEnd", InputActionType.Button, "<Keyboard>/Escape");
        funcGameRestartAction = new InputAction("FuncGameRestart", InputActionType.Button, "<Keyboard>/F5");
        funcSpeedUpAction = new InputAction("FuncSpeedUp", InputActionType.Button, $"<Keyboard>/{PlayerPrefs.GetString("SpeedUp1")}");
        funcSpeedDownAction = new InputAction("FuncSpeedDown", InputActionType.Button, $"<Keyboard>/{PlayerPrefs.GetString("SpeedDown1")}");
        funcSpeedUp2Action = new InputAction("FuncSpeedUp2", InputActionType.Button, $"<Keyboard>/{PlayerPrefs.GetString("SpeedUp2")}");
        funcSpeedDown2Action = new InputAction("FuncSpeedDown2", InputActionType.Button, $"<Keyboard>/{PlayerPrefs.GetString("SpeedDown2")}");
        funcJudgeAdjUpAction = new InputAction("FuncJudgeAdjUp", InputActionType.Button, "<Keyboard>/F8");
        funcJudgeAdjDownAction = new InputAction("FuncJudgeAdjDown", InputActionType.Button, "<Keyboard>/F7");
    }

    public void KeySetting()
    {
        MakeKeyAction(keyInputAction1, 0);
        MakeKeyAction(keyInputAction2, 1);
        MakeKeyAction(keyInputAction3, 2);
        MakeKeyAction(keyInputAction4, 3);
        MakeKeyAction(keyInputAction5, 4);
        if(isAssistKeyUse) { MakeKeyAction(keyInputAction6, 2); }

        MakeFunctionKeyAction();

        InputSystem.pollingFrequency = 1000.0f;
    }

    public void KeyDisable()
    {
        DeleteKeyAction(keyInputAction1, 0);
        DeleteKeyAction(keyInputAction2, 1);
        DeleteKeyAction(keyInputAction3, 2);
        DeleteKeyAction(keyInputAction4, 3);
        DeleteKeyAction(keyInputAction5, 4);
        if (isAssistKeyUse) { DeleteKeyAction(keyInputAction6, 2); }

        DeleteFunctionKeyAction();
    }

    private void MakeKeyAction(InputAction action, int index)
    {
        // 액션 이벤트 연결
        action.started += ctx => { bmsGameManager.KeyDown(index); };  // 눌렀을 때
        action.canceled += ctx => { bmsGameManager.KeyUp(index); };  // 뗐을 때

        // 액션 활성화
        action.Enable();
    }

    private void MakeFunctionKeyAction()
    {
        funcGameEndAction.started += ctx => { StartCoroutine(bmsGameManager.GameEnd(false)); };
        funcGameRestartAction.started += ctx => { StartCoroutine(bmsGameManager.GameRestart()); };
        funcSpeedUpAction.started += ctx => { bmsGameManager.ChangeSpeed(1); };
        funcSpeedDownAction.started += ctx => { bmsGameManager.ChangeSpeed(-1); };
        funcSpeedUp2Action.started += ctx => { bmsGameManager.ChangeSpeed(PlayerPrefs.GetInt("NoteSpeed")); };
        funcSpeedDown2Action.started += ctx => { bmsGameManager.ChangeSpeed(-(int)(PlayerPrefs.GetInt("NoteSpeed") * 0.5f)); };
        funcJudgeAdjUpAction.started += ctx => { bmsGameManager.ChangeJudgeAdjValue(1); };
        funcJudgeAdjDownAction.started += ctx => { bmsGameManager.ChangeJudgeAdjValue(-1); };

        funcGameEndAction.Enable();
        funcGameRestartAction.Enable();
        funcSpeedUpAction.Enable();
        funcSpeedDownAction.Enable();
        funcSpeedUp2Action.Enable();
        funcSpeedDown2Action.Enable();
        funcJudgeAdjUpAction.Enable();
        funcJudgeAdjDownAction.Enable();
    }

    private void DeleteKeyAction(InputAction action, int index)
    {
        action.started -= ctx => { bmsGameManager.KeyDown(index); };  // 눌렀을 때
        action.canceled -= ctx => { bmsGameManager.KeyUp(index); };  // 뗐을 때

        action.Disable();
        action = null;
    }

    private void DeleteFunctionKeyAction()
    {
        funcGameEndAction.started -= ctx => { StartCoroutine(bmsGameManager.GameEnd(false)); };
        funcGameRestartAction.started -= ctx => { StartCoroutine(bmsGameManager.GameRestart()); };
        funcSpeedUpAction.started -= ctx => { bmsGameManager.ChangeSpeed(1); };
        funcSpeedDownAction.started -= ctx => { bmsGameManager.ChangeSpeed(-1); };
        funcSpeedUp2Action.started -= ctx => { bmsGameManager.ChangeSpeed(PlayerPrefs.GetInt("NoteSpeed")); };
        funcSpeedDown2Action.started -= ctx => { bmsGameManager.ChangeSpeed(-(int)(PlayerPrefs.GetInt("NoteSpeed") * 0.5f)); };
        funcJudgeAdjUpAction.started -= ctx => { bmsGameManager.ChangeJudgeAdjValue(1); };
        funcJudgeAdjDownAction.started -= ctx => { bmsGameManager.ChangeJudgeAdjValue(-1); };

        funcGameEndAction.Disable();
        funcGameRestartAction.Disable();
        funcSpeedUpAction.Disable();
        funcSpeedDownAction.Disable();
        funcSpeedUp2Action.Disable();
        funcSpeedDown2Action.Disable();
        funcJudgeAdjUpAction.Disable();
        funcJudgeAdjDownAction.Disable();
        funcGameEndAction = null;
        funcGameRestartAction = null;
        funcSpeedUpAction = null;
        funcSpeedDownAction = null;
        funcSpeedUp2Action = null;
        funcSpeedDown2Action = null;
        funcJudgeAdjUpAction = null;
        funcJudgeAdjDownAction = null;
    }
}
