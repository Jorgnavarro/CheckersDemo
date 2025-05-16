using UnityEngine;

public class BackgroundMover : MonoBehaviour
{
    public RectTransform image1;
    public RectTransform image2;
    public float scrollSpeed = 100f;

    private float imageHeight;

    void Start()
    {
        // Get height of background adapted to screen
        imageHeight = image1.rect.height;

        // Position the second image just above the first
        image1.anchoredPosition = Vector2.zero;
        image2.anchoredPosition = new Vector2(0, imageHeight);
    }

    void Update()
    {
        float movement = scrollSpeed * Time.deltaTime;

        // Move both images down
        image1.anchoredPosition -= new Vector2(0, movement);
        image2.anchoredPosition -= new Vector2(0, movement);

        // If an image goes off the screen, reposition it on top
        if (image1.anchoredPosition.y <= -imageHeight)
        {
            image1.anchoredPosition = new Vector2(0, image2.anchoredPosition.y + imageHeight);
        }

        if (image2.anchoredPosition.y <= -imageHeight)
        {
            image2.anchoredPosition = new Vector2(0, image1.anchoredPosition.y + imageHeight);
        }
    }


    
}
