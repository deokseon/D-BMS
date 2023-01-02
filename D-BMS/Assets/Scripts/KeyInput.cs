using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class KeyInput : MonoBehaviour
{
    // 1번키부터 5번키까지 입력액션
    private InputAction keyInputAction1;
    private InputAction keyInputAction2;
    private InputAction keyInputAction3;
    private InputAction keyInputAction4;
    private InputAction keyInputAction5;

    private InputAction funcGameEndAction;
    private InputAction funcGameRestartAction;
    private InputAction funcSpeedUpAction;
    private InputAction funcSpeedDownAction;
    private InputAction funcJudgeAdjUpAction;
    private InputAction funcJudgeAdjDownAction;

    [SerializeField]
    private GameObject[] keyFeedback;
    [SerializeField]
    private SpriteRenderer[] keyboard;
    [SerializeField]
    private Sprite[] keyInitImage;
    [SerializeField]
    private Sprite[] keyPressedImage;

    [SerializeField]
    private SoundManager soundManager;
    [SerializeField]
    private BMSGameManager bmsGameManager;

    void Awake()
    {
        keyInputAction1 = new InputAction("KeyInput1", InputActionType.Button, $"<Keyboard>/{KeySettingManager.keyConfig.keys[0]}");
        keyInputAction2 = new InputAction("KeyInput2", InputActionType.Button, $"<Keyboard>/{KeySettingManager.keyConfig.keys[1]}");
        keyInputAction3 = new InputAction("KeyInput3", InputActionType.Button, $"<Keyboard>/{KeySettingManager.keyConfig.keys[2]}");
        keyInputAction4 = new InputAction("KeyInput4", InputActionType.Button, $"<Keyboard>/{KeySettingManager.keyConfig.keys[3]}");
        keyInputAction5 = new InputAction("KeyInput5", InputActionType.Button, $"<Keyboard>/{KeySettingManager.keyConfig.keys[4]}");

        funcGameEndAction = new InputAction("FuncGameEnd", InputActionType.Button, "<Keyboard>/Escape");
        funcGameRestartAction = new InputAction("FuncGameRestart", InputActionType.Button, "<Keyboard>/F5");
        funcSpeedUpAction = new InputAction("FuncSpeedUp", InputActionType.Button, "<Keyboard>/F2");
        funcSpeedDownAction = new InputAction("FuncSpeedDown", InputActionType.Button, "<Keyboard>/F1");
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

        DeleteFunctionKeyAction();
    }

    private void MakeKeyAction(InputAction action, int index)
    {
        // 액션 이벤트 연결
        action.started += ctx => {
            soundManager.PlayKeySound(bmsGameManager.currentNote[index]);
            bmsGameManager.KeyDown(index);
            keyFeedback[index].SetActive(true);
            keyboard[index].sprite = keyPressedImage[index % 2];
        };  // 눌렀을 때
        action.canceled += ctx => {
            keyFeedback[index].SetActive(false);
            bmsGameManager.KeyUp(index);
            keyboard[index].sprite = keyInitImage[index % 2];
        };  // 뗐을 때

        // 액션 활성화
        action.Enable();
    }

    private void MakeFunctionKeyAction()
    {
        funcGameEndAction.started += ctx => { StartCoroutine(bmsGameManager.GameEnd(false)); };
        funcGameRestartAction.started += ctx => { StartCoroutine(bmsGameManager.GameRestart()); };
        funcSpeedUpAction.started += ctx => { bmsGameManager.ChangeSpeed(0.1f); };
        funcSpeedDownAction.started += ctx => { bmsGameManager.ChangeSpeed(-0.1f); };
        funcJudgeAdjUpAction.started += ctx => { bmsGameManager.ChangeJudgeAdjValue(1); };
        funcJudgeAdjDownAction.started += ctx => { bmsGameManager.ChangeJudgeAdjValue(-1); };

        funcGameEndAction.Enable();
        funcGameRestartAction.Enable();
        funcSpeedUpAction.Enable();
        funcSpeedDownAction.Enable();
        funcJudgeAdjUpAction.Enable();
        funcJudgeAdjDownAction.Enable();
    }

    private void DeleteKeyAction(InputAction action, int index)
    {
        action.started -= ctx => {
            soundManager.PlayKeySound(bmsGameManager.currentNote[index]);
            bmsGameManager.KeyDown(index);
            keyFeedback[index].SetActive(true);
            keyboard[index].sprite = keyPressedImage[index % 2];
        };  // 눌렀을 때
        action.canceled -= ctx => {
            keyFeedback[index].SetActive(false);
            bmsGameManager.KeyUp(index);
            keyboard[index].sprite = keyInitImage[index % 2];
        };  // 뗐을 때

        action.Disable();
        action = null;
    }

    private void DeleteFunctionKeyAction()
    {
        funcGameEndAction.started -= ctx => { StartCoroutine(bmsGameManager.GameEnd(false)); };
        funcGameRestartAction.started -= ctx => { StartCoroutine(bmsGameManager.GameRestart()); };
        funcSpeedUpAction.started -= ctx => { bmsGameManager.ChangeSpeed(0.1f); };
        funcSpeedDownAction.started -= ctx => { bmsGameManager.ChangeSpeed(-0.1f); };
        funcJudgeAdjUpAction.started += ctx => { bmsGameManager.ChangeJudgeAdjValue(1); };
        funcJudgeAdjDownAction.started += ctx => { bmsGameManager.ChangeJudgeAdjValue(-1); };

        funcGameEndAction.Disable();
        funcGameRestartAction.Disable();
        funcSpeedUpAction.Disable();
        funcSpeedDownAction.Disable();
        funcJudgeAdjUpAction.Disable();
        funcJudgeAdjDownAction.Disable();
        funcGameEndAction = null;
        funcGameRestartAction = null;
        funcSpeedUpAction = null;
        funcSpeedDownAction = null;
        funcJudgeAdjUpAction = null;
        funcJudgeAdjDownAction = null;
    }
}
