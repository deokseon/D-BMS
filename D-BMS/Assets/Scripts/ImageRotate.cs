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
}
