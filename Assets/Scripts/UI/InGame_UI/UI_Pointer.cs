using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_Pointer : MonoBehaviour
{
    private UI ui;
    private Player_Interaction interaction;

    [SerializeField] private Image fillPointer;
    private Coroutine fillCo;

    private void Awake()
    {
        interaction = FindFirstObjectByType<Player_Interaction>();
    }

    private void Start()
    {
        ui = GetComponentInParent<UI>();
        interaction.OnReleasedLMB += CancelFill; // Cancel fill when player releases LMB

        CancelFill();
    }

    public void BeginToFeelPointer(float duration)
    {
        if(fillCo != null)
            StopCoroutine(fillCo);

        fillCo = StartCoroutine(FillRadialImage(duration));
    }

    public void CancelFill()
    {
        if(fillCo != null)
            StopCoroutine(fillCo);

        fillPointer.gameObject.SetActive(false);
        fillPointer.fillAmount = 0;
    }

    private IEnumerator FillRadialImage(float duration)
    {
        fillPointer.gameObject.SetActive(true);
        float elapsedTime = 0;
        float startValue = fillPointer.fillAmount;



        while (elapsedTime < duration)
        {                               // Gives you time it took from last frame to current one - In seconds
            elapsedTime = elapsedTime + Time.deltaTime;

            fillPointer.fillAmount = Mathf.Lerp(startValue, 1, elapsedTime / duration);
            yield return null;

        }

        fillPointer.fillAmount = 1;
    }
}
