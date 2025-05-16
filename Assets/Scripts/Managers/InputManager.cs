using UnityEngine;

public class InputManager : MonoBehaviour
{
    
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            TouchDetection();
        }
        else
        {
            ClickDetection();
        }
    }

    private void ClickDetection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ProcessSelection(Input.mousePosition);
        }
    }

    private void TouchDetection()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
           ProcessSelection(Input.GetTouch(0).position); 
        }
    }

    private void ProcessSelection(Vector2 screenPoint)
    {
        Debug.Log("Input detected in position: " + screenPoint);
    }
}
