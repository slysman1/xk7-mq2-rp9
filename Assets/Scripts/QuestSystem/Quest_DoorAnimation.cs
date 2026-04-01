using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Quest_DoorAnimation : MonoBehaviour
{
    private Quest_MainNPC mainNpc;

    [SerializeField] private Vector3 openDoorRot = new Vector3(0, -15, 0);
    [SerializeField] private float closeDoorDuration = .2f;

    private void Awake()
    {
        mainNpc = GetComponentInParent<Quest_MainNPC>();
    }

    public void OpenDoor(float deliveryDuration)
    {
        Audio.PlaySFX("delivery_door_open", transform);
        Audio.PlaySFX("delivery_door_squeak", transform);
        StartCoroutine(RotateLocal(transform, openDoorRot, deliveryDuration));

    }

    public IEnumerator CloseDoorCo(bool deliveryIsCorrect)
    {
        string closeDoorSFX = deliveryIsCorrect ? "delivery_door_close_accept" : "delivery_door_close_reject";

        Audio.PlaySFX(closeDoorSFX, mainNpc.soundSource);
        yield return RotateLocal(transform, -openDoorRot, closeDoorDuration);
    }
}
