using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Quest_WindowAnimation : MonoBehaviour
{

    public bool windowOpen { get; private set; }

    [Header("Guard Window")]
    [SerializeField] private Transform window;
    [SerializeField] private Vector3 windowOpenPosition;
    [SerializeField] private float windowOpenDuration;
    [SerializeField] private float windowCloseDuration = .1f;


    public IEnumerator OpenWindowCo()
    {
        Audio.PlaySFX("delivery_door_window_squeek", transform);
        windowOpen = true;
        yield return StartCoroutine(MoveLocalPosition(window, windowOpenPosition, windowOpenDuration));
    }


    public IEnumerator CloseWindowCo(float closeDoorDelay = 0)
    {

        yield return new WaitForSeconds(closeDoorDelay);
        yield return StartCoroutine(MoveLocalPosition(window, Vector3.zero, windowCloseDuration));
        windowOpen = false;
    }
}
