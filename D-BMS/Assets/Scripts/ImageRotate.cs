using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageRotate : MonoBehaviour
{
    private enum RotateDirection { clockwise, counterclockwise }
    [SerializeField]
    private RotateDirection direction;
    [SerializeField]
    private float rotateSpeed;
    private RectTransform rotateImage;

    
    void Awake()
    {
        rotateImage = gameObject.GetComponent<RectTransform>();
        rotateSpeed *= (direction == RotateDirection.clockwise ? -1 : 1);
    }

    void Update()
    {
        rotateImage.Rotate(new Vector3(0, 0, rotateSpeed * Time.deltaTime));
    }

    public void SetRotateSpeed()
    {
        rotateSpeed = rotateSpeed != 0.0f ? 0.0f : 36.0f * (direction == RotateDirection.clockwise ? -1 : 1);
        rotateImage.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
    }
}
