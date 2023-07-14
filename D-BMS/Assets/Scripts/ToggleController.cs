using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour, IPointerDownHandler
{
	public string toggleTitle;

	private bool isOn;

	public Color onColorBg;
	public Color offColorBg;

	public Image toggleBgImage;
	public RectTransform toggle;

	public GameObject handle;
	private RectTransform handleTransform;

	private float handleSize;
	private float onPosX;
	private float offPosX;

	public float handleOffset;

	public float speed;
	static float t = 0.0f;

	[HideInInspector] public bool switching = false;

	void Awake()
	{
		handleTransform = handle.GetComponent<RectTransform>();
		RectTransform handleRect = handle.GetComponent<RectTransform>();
		handleSize = handleRect.sizeDelta.x;
		float toggleSizeX = toggle.sizeDelta.x;
		onPosX = (toggleSizeX / 2) - (handleSize/2) - handleOffset;
		offPosX = onPosX * -1;
		isOn = PlayerPrefs.GetInt(toggleTitle) == 1 ? true : false;
	}

	void Start()
	{
		if(isOn)
		{
			toggleBgImage.color = onColorBg;
			handleTransform.localPosition = new Vector3(onPosX, 0f, 0f);
		}
		else
		{
			toggleBgImage.color = offColorBg;
			handleTransform.localPosition = new Vector3(offPosX, 0f, 0f);
		}
	}
		
	void Update()
	{
		if(switching)
		{
			Toggle(isOn);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			switching = true;
		}
	}

	public void Toggle(bool toggleStatus)
	{
		if(toggleStatus)
		{
			toggleBgImage.color = SmoothColor(onColorBg, offColorBg);
			handleTransform.localPosition = SmoothMove(handle, onPosX, offPosX);
		}
		else 
		{
			toggleBgImage.color = SmoothColor(offColorBg, onColorBg);
			handleTransform.localPosition = SmoothMove(handle, offPosX, onPosX);
		}
			
	}

	Vector3 SmoothMove(GameObject toggleHandle, float startPosX, float endPosX)
	{
		
		Vector3 position = new Vector3 (Mathf.Lerp(startPosX, endPosX, t += speed * Time.deltaTime), 0f, 0f);
		StopSwitching();
		return position;
	}

	Color SmoothColor(Color startCol, Color endCol)
	{
		Color resultCol;
		resultCol = Color.Lerp(startCol, endCol, t += speed * Time.deltaTime);
		return resultCol;
	}

	CanvasGroup Transparency (GameObject alphaObj, float startAlpha, float endAlpha)
	{
		CanvasGroup alphaVal;
		alphaVal = alphaObj.gameObject.GetComponent<CanvasGroup>();
		alphaVal.alpha = Mathf.Lerp(startAlpha, endAlpha, t += speed * Time.deltaTime);
		return alphaVal;
	}

	void StopSwitching()
	{
		if(t > 1.0f)
		{
			switching = false;

			t = 0.0f;
			isOn = !isOn;

			PlayerPrefs.SetInt(toggleTitle, isOn ? 1 : 0);
		}
	}
}
