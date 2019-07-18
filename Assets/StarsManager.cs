using UnityEngine;


public class StarsManager : MonoBehaviour
{
    public GameObject[] winPanelStars;

    private int collectedStarsCount = 0;

    private const int MAXIMUM_STARS_COUNT = 3;

    public void ResetStars()
    {
        collectedStarsCount = 0;

        for (int i = 0; i < MAXIMUM_STARS_COUNT; i++)
        {
            winPanelStars[i].SetActive(false);
        }
    }

    public void StarCollected()
    {
        if (collectedStarsCount > MAXIMUM_STARS_COUNT - 1)
        {
            return;
        }

        winPanelStars[collectedStarsCount].SetActive(true);

        collectedStarsCount++;
    }

}