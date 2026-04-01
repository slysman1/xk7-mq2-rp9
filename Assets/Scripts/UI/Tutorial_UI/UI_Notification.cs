using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Alexdev.TweenUtils;


public class UI_Notification : MonoBehaviour
{
    private AudioListener audioListener;
    private RectTransform rect;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private float moveShowDurration = .4f;
    [SerializeField] private float showDuration = 1f;
    [SerializeField] private float hideDuration = 1f;
    [SerializeField] private float moveDistance;
    [SerializeField] private Vector3 moveDir;

    private Vector2 defaultPosition;
    public bool isShowingNotification { get; private set; }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        defaultPosition = rect.anchoredPosition;    
    }

    private void Start()
    {
        audioListener = FindFirstObjectByType<AudioListener>();
        HideNotification(true);
    }



    public void NotifyPlayer(string key, bool canRepeatNotification = false)
    {
        if (canRepeatNotification == false)
        {
            if (NotificationSession.CanShow(key) == false)
                return;
        }

        StartCoroutine(NotifyPlayerCo(key));
    }


    private IEnumerator NotifyPlayerCo(string key)
    {
        if (isShowingNotification)
            yield return HideNotificationCo(true);

        yield return null;


        text.text = Localization.GetString(key);
        isShowingNotification = true;

        Vector2 target = defaultPosition + new Vector2(moveDir.x,moveDir.y) * moveDistance;

        Audio.QueSFX("ui_tutorial_tip_show_up", audioListener.transform,showDuration - .1f);
        StartCoroutine(MoveUI(rect, target, showDuration));
        StartCoroutine(SetCanvasAlphaTo(canvasGroup, 1, showDuration));
    }

    public void HideNotification(bool quickHide = false)
    {
        StartCoroutine(HideNotificationCo(quickHide));
    }

    private IEnumerator HideNotificationCo(bool quickHide = false)
    {
        isShowingNotification = false;
        Vector2 target = rect.anchoredPosition + Vector2.down * moveDistance;

        float newHideDurration = quickHide ? 0.01f : hideDuration;
        float newAudioDelay = newHideDurration > 0.01f ? hideDuration - .1f : 0f;

        Audio.QueSFX("ui_tutorial_tip_show_up", audioListener.transform, newAudioDelay);
        StartCoroutine(MoveUI(rect, target, 1f));
        StartCoroutine(SetCanvasAlphaTo(canvasGroup, 0, newHideDurration));
        
        yield return new WaitForSeconds(hideDuration);

        rect.anchoredPosition = defaultPosition;
    }
}
