using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class KeyInput : MonoBehaviour
{
    // 1번키부터 5번키까지 입력액션
    private InputAction keyLineAction1;
    private InputAction keyLineAction2;
    private InputAction keyLineAction3;
    private InputAction keyLineAction4;
    private InputAction keyLineAction5;

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

    private char[] keyMapping = { 'a', 'w', 'j', 'i', 'l' };

    void Awake()
    {
        keyLineAction1 = new InputAction($"Key Line 1", InputActionType.Button, $"<Keyboard>/{keyMapping[0]}");
        keyLineAction2 = new InputAction($"Key Line 2", InputActionType.Button, $"<Keyboard>/{keyMapping[1]}");
        keyLineAction3 = new InputAction($"Key Line 3", InputActionType.Button, $"<Keyboard>/{keyMapping[2]}");
        keyLineAction4 = new InputAction($"Key Line 4", InputActionType.Button, $"<Keyboard>/{keyMapping[3]}");
        keyLineAction5 = new InputAction($"Key Line 5", InputActionType.Button, $"<Keyboard>/{keyMapping[4]}");
    }

    public void KeySetting()
    {
        MakeKeyAction(keyLineAction1, 0);
        MakeKeyAction(keyLineAction2, 1);
        MakeKeyAction(keyLineAction3, 2);
        MakeKeyAction(keyLineAction4, 3);
        MakeKeyAction(keyLineAction5, 4);

        InputSystem.pollingFrequency = 1000.0f;
    }

    public void KeyDisable()
    {
        DeleteKeyAction(keyLineAction1, 0);
        DeleteKeyAction(keyLineAction2, 1);
        DeleteKeyAction(keyLineAction3, 2);
        DeleteKeyAction(keyLineAction4, 3);
        DeleteKeyAction(keyLineAction5, 4);
    }

    void MakeKeyAction(InputAction action, int index)
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

    void DeleteKeyAction(InputAction action, int index)
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
}
