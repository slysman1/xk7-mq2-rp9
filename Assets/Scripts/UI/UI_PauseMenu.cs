using UnityEngine;

public class UI_PauseMenu : MonoBehaviour
{
    public CanvasGroup canvasGroup {  get; private set; }
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

}
