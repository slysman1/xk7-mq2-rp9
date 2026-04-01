using UnityEngine;

public enum PlacementDirection { Left, Right, Backward, Forward,None }

[ExecuteAlways]
public class Environment_Column : MonoBehaviour
{
    [Header("Placement Direction")]
    [SerializeField] private PlacementDirection allowedDirection = PlacementDirection.None;

    [Header("Blockers")]
    [SerializeField] private Transform blockerA;
    [SerializeField] private Transform blockerB;

    [Header("Offset Settings")]
    [SerializeField] private float offsetDistance = 1f;

    private void OnValidate()
    {
        UpdateBlockerPositions();
    }

    private void UpdateBlockerPositions()
    {
        if (!blockerA || !blockerB)
            return;

        Vector3 offsetA = Vector3.zero;
        Vector3 offsetB = Vector3.zero;

        switch (allowedDirection)
        {
            case PlacementDirection.Left:
                offsetA = Vector3.right * offsetDistance;
                offsetB = Vector3.right * offsetDistance;
                break;
            case PlacementDirection.Right:
                offsetA = Vector3.left * offsetDistance;
                offsetB = Vector3.left * offsetDistance;
                break;
            case PlacementDirection.Backward:
                offsetA = Vector3.forward * offsetDistance;
                offsetB = Vector3.forward * offsetDistance;
                break;
            case PlacementDirection.Forward:
                offsetA = Vector3.back * offsetDistance;
                offsetB = Vector3.back * offsetDistance;
                break;
            case PlacementDirection.None:
                offsetA = Vector3.zero;
                offsetB = Vector3.zero;
                break;
        }

        // Preserve original Y position
        blockerA.localPosition = new Vector3(offsetA.x, blockerA.localPosition.y, offsetA.z);
        blockerB.localPosition = new Vector3(offsetB.x, blockerB.localPosition.y, offsetB.z);
    }
}
