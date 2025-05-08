using UnityEngine;

public class Box : MonoBehaviour
{
    public int row, col;
    
    void Start()
    {
        CalculateGridPosition();
    }

    private void CalculateGridPosition()
    {
        row = Mathf.FloorToInt(transform.position.y + 4);
        col = Mathf.FloorToInt(transform.position.x + 4);
    }
}
