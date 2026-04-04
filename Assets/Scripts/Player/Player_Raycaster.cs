using System.Collections;
using UnityEngine;

public class Player_Raycaster : MonoBehaviour
{
    public Camera cam { get; private set; }
    private Player player;

    [Header("Ranges")]
    [SerializeField] private float updateRate = 0.1f;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private float previewRange = 5f;
    private float defaultUpdateRate;

    [Header("Masks")]
    [SerializeField] private LayerMask whatIsInteractable;
    [SerializeField] private LayerMask whatIsHighlightable;
    [SerializeField] private LayerMask whatIsHoverable;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsPlacementBlocker;

    public LayerMask PlacementBlockerMask => whatIsPlacementBlocker;
    public LayerMask CombinedMask => whatIsInteractable | whatIsWall | whatIsGround | whatIsPlacementBlocker;
    public LayerMask InteractableMask => whatIsInteractable;
    public LayerMask WallMask => whatIsWall;
    public float InteractionRange => interactionRange;
    public float PreviewRange => previewRange;
    public Vector3 HitPoint => hasInteractableHit ? interactableHit.point : Vector3.zero;

    // Cached results
    public RaycastHit interactableHit { get; private set; }
    public bool hasInteractableHit { get; private set; }

    public RaycastHit combinedHit { get; private set; }
    public bool hasCombinedHit { get; private set; }

    public RaycastHit highlightHit { get; private set; }
    public bool hasHighlightHit { get; private set; }

    public RaycastHit hoverHit { get; private set; }
    public bool hasHoverHit { get; private set; }

    public RaycastHit floorHit { get; private set; }
    public bool hasFloorHit { get; private set; }
    private void Start()
    {
        player = GetComponent<Player>();
        cam = Camera.main;

        defaultUpdateRate = updateRate;
        StartCoroutine(UpdateLoop());
    }

    private IEnumerator UpdateLoop()
    {
        while (true)
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            UpdateInteractableHit(ray);
            UpdateCombinedHit(ray);
            UpdateHighlightHit(ray);
            UpdateHoverHit(ray);
            yield return new WaitForSeconds(updateRate);
        }
    }

    public void SetUpdateRate(float rate) => updateRate = rate;
    public void SetDefaultUpdateRate() => updateRate = defaultUpdateRate;

    // --- Cached updaters ---

    private void UpdateInteractableHit(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, whatIsInteractable, QueryTriggerInteraction.Ignore))
        {
            interactableHit = hit;
            hasInteractableHit = true;
            return;
        }

        if (Physics.Raycast(ray, out hit, interactionRange, whatIsInteractable, QueryTriggerInteraction.Collide))
        {
            interactableHit = hit;
            hasInteractableHit = true;
            return;
        }

        hasInteractableHit = false;
    }

    private void UpdateCombinedHit(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, previewRange, CombinedMask))
        {
            combinedHit = hit;
            hasCombinedHit = true;
            return;
        }

        hasCombinedHit = false;
    }

    private void UpdateHighlightHit(Ray ray)
    {
        RaycastHit[] solidHits = Physics.RaycastAll(cam.transform.position, cam.transform.forward,
            interactionRange, whatIsHighlightable, QueryTriggerInteraction.Ignore);

        if (solidHits.Length > 0)
        {
            System.Array.Sort(solidHits, (a, b) => a.distance.CompareTo(b.distance));
            highlightHit = solidHits[0];
            hasHighlightHit = true;
            return;
        }

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, whatIsHighlightable, QueryTriggerInteraction.Collide))
        {
            highlightHit = hit;
            hasHighlightHit = true;
            return;
        }

        hasHighlightHit = false;
    }

    private void UpdateHoverHit(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, whatIsHoverable))
        {
            hoverHit = hit;
            hasHoverHit = true;
            return;
        }

        hasHoverHit = false;
    }
    public void ForceUpdate()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        UpdateInteractableHit(ray);
        UpdateCombinedHit(ray);
        UpdateHighlightHit(ray);
        UpdateHoverHit(ray);
    }

    // --- On-demand fresh raycasts for interaction/preview ---

    public bool HitInteractable(out RaycastHit hit)
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        UpdateInteractableHit(ray);
        hit = interactableHit;
        return hasInteractableHit;
    }

    public bool GetFreshCombinedHit(out RaycastHit hit)
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        UpdateCombinedHit(ray);
        hit = combinedHit;
        return hasCombinedHit;
    }

    public bool HitCombined(out RaycastHit hit)
    {
        hit = combinedHit;
        return hasCombinedHit;
    }

    public bool HitFloor(Vector3 previewPosition, out RaycastHit hit)
    {
        UpdateFloorHitFromPosition(previewPosition);
        hit = floorHit;
        return hasFloorHit;
    }

    public bool HitWall(out RaycastHit hit)
    {
        hit = combinedHit;
        return hasCombinedHit
            && ((1 << combinedHit.collider.gameObject.layer) & whatIsWall) != 0
            && Mathf.Abs(combinedHit.normal.y) < 0.5f;
    }
    public bool HitHighlight(out RaycastHit hit)
    {
        hit = highlightHit;
        return hasHighlightHit;
    }

    public bool HitHover(out RaycastHit hit)
    {
        hit = hoverHit;
        return hasHoverHit;
    }

    public bool HitPlacementBlocker(RaycastHit hit)
    {
        return ((1 << hit.collider.gameObject.layer) & whatIsPlacementBlocker) != 0;
    }

    // --- Floor hit for VFX (called by PreviewHandler) ---

    public void UpdateFloorHitFromPosition(Vector3 previewPosition)
    {
        Vector3 rayStart = previewPosition + Vector3.up * 0.5f;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 5f, CombinedMask))
        {
            floorHit = hit;
            hasFloorHit = true;
            return;
        }

        hasFloorHit = false;
    }

    public Vector3 GetWallPushVector(float range, float pushOffset)
    {
        Vector3 origin = cam.transform.position;

        if (Physics.Raycast(origin, cam.transform.forward, out RaycastHit hit, range, whatIsWall | whatIsPlacementBlocker))
        {
            Vector3 normal = hit.normal;
            normal.y = 0f;
            normal.Normalize();
            float pushAmount = pushOffset - hit.distance + (range - pushOffset);
            return normal * Mathf.Max(0, pushOffset - hit.distance + (range - hit.distance) * 0.1f);
        }

        return Vector3.zero;
    }
}