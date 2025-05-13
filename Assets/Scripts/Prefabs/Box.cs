using UnityEngine;

public class Box : MonoBehaviour
{
    public int row, col;
    public GameObject moveIndicator;
    
    void Start()
    {
        CalculateGridPosition();
        moveIndicator.SetActive(false);
    }

    private void CalculateGridPosition()
    {
        row = Mathf.FloorToInt(transform.position.y + 4);
        col = Mathf.FloorToInt(transform.position.x + 4);
    }

    public void ShowMoveIndicator(bool show, Color? color = null)
    {
        moveIndicator.SetActive(show);
        if (show && color.HasValue)
        {
            moveIndicator.GetComponent<SpriteRenderer>().color = color.Value;
        }
    }
}
