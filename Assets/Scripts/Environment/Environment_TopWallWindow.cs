using UnityEngine;
public enum WindowType { First, Second, Third,None }

public class Environment_TopWallWindow : MonoBehaviour
{
    [SerializeField] private WindowType windowType;
    [SerializeField] private GameObject[] windowVariants;


    private void OnValidate()
    {
        UpdateWindow(windowType);
    }

    public void UpdateWindow(WindowType windowType)
    {
        this.windowType = windowType;

        foreach (var window in windowVariants)
            window.SetActive(false);

        switch (windowType)
        {
            case WindowType.First:
                windowVariants[0].SetActive(true);
                break;

            case WindowType.Second:
                windowVariants[1].SetActive(true);
                break;

            case WindowType.Third:
                windowVariants[2].SetActive(true);
                break;
        }
    }
}
