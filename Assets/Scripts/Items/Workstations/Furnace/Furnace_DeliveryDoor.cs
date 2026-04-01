using UnityEngine;
using static Alexdev.TweenUtils;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class Furnace_DeliveryDoor : MonoBehaviour
{

    [SerializeField] private bool openByDefault = false;

    [SerializeField] private Vector3 closedDoorRot;
    [SerializeField] private Vector3 openDoorRot;
    private bool doorOpen;
    private Coroutine doorRoutine;

    private void Awake()
    {
        if (openByDefault)
            OpenDoor();
    }

    public void OpenDoor(float duration = 0.25f) => OpenDoor(true, duration);
    public void CloseDoor(float duration = 0.15f) => OpenDoor(false, duration);

    private void OpenDoor(bool openDoor, float duration)
    {
        if (doorRoutine != null)
            StopCoroutine(doorRoutine);

        doorOpen = openDoor;
        Vector3 doorRotation = openDoor ? openDoorRot : closedDoorRot;
        string sound = openDoor ? "producton_door_open" : "producton_door_close";
        float soundDelay = openDoor ? 0f : duration;
        Audio.PlaySFX(sound, transform);//, duration);

        doorRoutine = StartCoroutine(SetLocalRotationAs(transform, doorRotation, duration));
    }


#if UNITY_EDITOR

    [ContextMenu("DOOR OPEN: Save rotation")]
    public void SaveOpenDoorRotation()
    {
        SerializedObject so = new SerializedObject(transform);
        SerializedProperty rot = so.FindProperty("m_LocalEulerAnglesHint");

        openDoorRot = rot.vector3Value;
    }

    [ContextMenu("DOOR CLOSE: Save rotation")]
    public void SaveCloseDoorRotation()
    {
        SerializedObject so = new SerializedObject(transform);
        SerializedProperty rot = so.FindProperty("m_LocalEulerAnglesHint");

        closedDoorRot = rot.vector3Value;
    }
#endif

}
