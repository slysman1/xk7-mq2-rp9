using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Player_PreviewHandler : MonoBehaviour
{
    
    private Player player;
    private Player_Interaction interaction;
    private Player_PreviewObject previewObject;
    private Player_Inventory inventory;
    private Player_Raycaster raycaster;

    private Item_Base itemBeingPlaced;


    public Vector2 previewRotateInput { get; private set; }
    private Vector3 lastWallPosition;

    [Header("Previw Controll Settings")]
    [SerializeField] private float itemIgnoreDur = 0.5f;
    [SerializeField] private float previewSmoothnes = 10f;
    [SerializeField] private float scrollRotationSpeed = 10f;

    [Header("Preview Distance")]
    [SerializeField] private float wallSnapOffset = .2f;
    [SerializeField] private float upPreviewDist = 0.05f;
    [SerializeField] private float forwardPreviewDist = .5f;
    private float currentDistance;

    private float scrollSpeed => scrollRotationSpeed * 100;

    private Quaternion currentPreviewRotation = Quaternion.identity;
    
    public bool isPlacingItem { get; private set; }



    

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    [Header("Placement Point VFX")]
    [SerializeField] private Transform previwPointVFX;

    [Header("Line Renderer VFX")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float lineRendererSpeed = 0.5f; // add this
    [SerializeField] private float lineRendererFadeInDelay = 0.5f;
    [SerializeField] private float lineRendererFadeInDuration = 0.3f;
    private Vector3 lineTopPoint;
    private Vector3 lineBottomPoint; // add this
    private Gradient lineRendererOriginalColorGradient;
    private Coroutine colorFadeCo;

    [Header("Snap To Holder Details")]
    private bool isSnapedToHolder = false;
    private Coroutine snapToHolderCo;
    



    private void Start()
    {
        player = GetComponent<Player>();
        raycaster = player.raycaster;
        interaction = player.interaction;
        
        inventory = player.inventory;
        lineRendererOriginalColorGradient = lineRenderer.colorGradient;

        player.input.Player.RotatePreview.performed += ctx => previewRotateInput = ctx.ReadValue<Vector2>();
        player.input.Player.RotatePreview.canceled += ctx => previewRotateInput = Vector2.zero;

        previewObject = GetComponentInChildren<Player_PreviewObject>(true);

        EnableLineRendererColor(false);
    }


    private void Update()
    {
        UpdatePreview();
        ShowPlacementPointIfNeeded();
        UpdateLineRendererPositions();
    }

    public void TryPlacementMode()
    {
        itemBeingPlaced = player.inventory.GetTopItem();

        if (itemBeingPlaced == null)
            return;

        raycaster.ForceUpdate();
        raycaster.SetUpdateRate(0.01f);
        EnableLineRendererColor(true);

        originalLocalPosition = itemBeingPlaced.transform.localPosition;
        originalLocalRotation = itemBeingPlaced.transform.localRotation;


        currentPreviewRotation = itemBeingPlaced.transform.rotation;

        Vector3 dirToPlayer = transform.position - itemBeingPlaced.transform.position;
        dirToPlayer.y = 0f;
        currentPreviewRotation = Quaternion.LookRotation(dirToPlayer) * Quaternion.Euler(0f, -40f, 0f);


        // world rot
        previewObject.EnablePreviw(itemBeingPlaced);
        previewObject.SetRotation(currentPreviewRotation);

        isPlacingItem = true;
        //UpdatePreview();

        UI.instance.inputHelp.RemoveInput();
        UI.instance.inputHelp.AddInput(KeyType.LMB, "input_drop_placement_confirm");
    }

    public void FinalizePlacement()
    {
        if (!isPlacingItem || itemBeingPlaced == null)
            return;


        if (interaction.CanAddItemsToHolder(out var holder))
        {
            if (holder.CanSnapPreview() == false)
            {
                // treat as if no holder, just drop
                player.inventory.PlaceItem(itemBeingPlaced, GetPreviewPosition(), currentPreviewRotation, itemIgnoreDur);
                isPlacingItem = false;
                StopPlacement();
                return;
            }

            Item_Base itemToPlace = itemBeingPlaced;
            StopPlacement();
            inventory.AddSingleItemDirectlyToHolder(itemToPlace, holder);
            return;
        }

        UpdatePreview();

        if (IsHoldingWallItem() && raycaster.HitWall(out _) == false)
            inventory.InstantReleaseAllItems(GetQuickDropPosition());
        else
            player.inventory.PlaceItem(itemBeingPlaced, GetPreviewPosition(), currentPreviewRotation, itemIgnoreDur);


        isPlacingItem = false;

        if (inventory.HasItemInHands())
        {
            TryPlacementMode();
        }
        else
            StopPlacement();
    }

    public void CancelPlacement()
    {
        if (isPlacingItem == false)
            return;

        itemBeingPlaced.EnableCamPriority(true);
        itemBeingPlaced.transform.parent = inventory.GetCarryPoint();
        itemBeingPlaced.transform.localPosition = originalLocalPosition;
        itemBeingPlaced.transform.localRotation = originalLocalRotation;

        itemBeingPlaced = null;
        isPlacingItem = false;
        previewObject.DisablePreview();
        EnableLineRendererColor(false);

        UI.instance.inputHelp.RemoveInputByKey(KeyType.LMB);
        UI.instance.inputHelp.Refresh();
    }

    private void StopPlacement()
    {

        raycaster.SetDefaultUpdateRate();
        itemBeingPlaced = null;
        isPlacingItem = false;
        previewObject.DisablePreview();
        EnableLineRendererColor(false);
        UI.instance.inputHelp.RemoveInputByKey(KeyType.LMB);
    }

    private void UpdatePreview()
    {
        if (!isPlacingItem || itemBeingPlaced == null)
            return;

        if (interaction.CanAddItemsToHolder(out ItemHolder holder) && holder.CanSnapPreview())
        {
            Transform slotTransform = holder.GetFreeSlot();

            if (slotTransform != null && !isSnapedToHolder)
            {
                isSnapedToHolder = true;
                itemBeingPlaced.transform.parent = null;

                if (snapToHolderCo != null)
                    StopCoroutine(snapToHolderCo);

                previwPointVFX.gameObject.SetActive(false);
                snapToHolderCo = StartCoroutine(SnapItemToHolder(slotTransform));
                StartCoroutine(SetRotationAs(itemBeingPlaced.transform, slotTransform.rotation.eulerAngles, .2f));
                
            }
        }
        else if (isSnapedToHolder)
        {
            itemBeingPlaced.transform.parent = inventory.GetCarryPoint();
            isSnapedToHolder = false;

            if (snapToHolderCo != null)
                StopCoroutine(snapToHolderCo);

            snapToHolderCo = StartCoroutine(ReturnItemToPreview());
            previwPointVFX.gameObject.SetActive(true);
        }
        else
        {
            UpdatePreviwPosition();
            UpdatePreviewRotation();
        }
    }


    private void UpdatePreviwPosition()
    {
        if (isSnapedToHolder)
            return;

        Vector3 lastPreviwPosition = GetPreviewPosition();
        //lastPreviewPosition = GetPreviewPosition();
        //previewObject.SetPosition(lastPreviwPosition);


        previewObject.transform.position = lastPreviwPosition;
        itemBeingPlaced.transform.position = lastPreviwPosition;
        itemBeingPlaced.transform.rotation = currentPreviewRotation;

    }


    private Vector3 GetPreviewPosition()
    {
        if (itemBeingPlaced == null)
            return Vector3.zero;

        Vector3 origin = raycaster.cam.transform.position;
        Vector3 dir = raycaster.cam.transform.forward;

        bool isWallItem = itemBeingPlaced.itemData.placementType == PlacementType.WallOnly;

        float forwardPreviewDistance = this.forwardPreviewDist + itemBeingPlaced.itemData.placementForwardOffset;

        if (raycaster.HitCombined(out RaycastHit hit))
        {
            if (isWallItem && raycaster.HitWall(out RaycastHit wallHit))
            {
                Vector3 targetWallPos = wallHit.point + wallHit.normal * wallSnapOffset;
                lastWallPosition = Vector3.Lerp(lastWallPosition, targetWallPos, Time.deltaTime * previewSmoothnes);
                return lastWallPosition;
            }

            // Floor item logic
            float hitDistance = Vector3.Distance(origin, hit.point);
            float targetDistance = Mathf.Min(hitDistance, forwardPreviewDistance);
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * previewSmoothnes);
            return origin + dir * currentDistance + Vector3.up * upPreviewDist;
        }

        // No hit — float in front of camera
        currentDistance = Mathf.Lerp(currentDistance, forwardPreviewDistance, Time.deltaTime * previewSmoothnes);
        return origin + dir * currentDistance + Vector3.up * upPreviewDist;
    }


    private void UpdatePreviewRotation()
    {
        if (isSnapedToHolder)
            return;

        if (itemBeingPlaced == null)
            return;

        if (raycaster.HitWall(out RaycastHit wallHit) && IsHoldingWallItem())
        {
            Vector3 flatNormal = wallHit.normal;
            flatNormal.y = 0f;
            flatNormal.Normalize();

            currentPreviewRotation = Quaternion.LookRotation(flatNormal);
            previewObject.SetRotation(currentPreviewRotation);
            return;
        }

        float delta = previewRotateInput.x + previewRotateInput.y;

        if (Mathf.Abs(delta) < Mathf.Epsilon)
            return;

        currentPreviewRotation *= Quaternion.Euler(0f, delta * scrollSpeed * Time.deltaTime, 0f);
        previewObject.SetRotation(currentPreviewRotation);
    }

    public Vector3 GetQuickDropPosition()
    {
        if (itemBeingPlaced == null)
            itemBeingPlaced = inventory.GetTopItem();


        return GetPreviewPosition();
    }


    
    public bool CanPlaceItem()
    {
        return itemBeingPlaced != null && isPlacingItem;// && previewObject.ItemCanBePlaced();
    }

    private void ShowPlacementPointIfNeeded()
    {
        if (!isPlacingItem || itemBeingPlaced == null)
        {
            if (previwPointVFX.gameObject.activeSelf)
                previwPointVFX.gameObject.SetActive(false);
            return;
        }


        // When snapped to holder, place VFX directly on item
        if (isSnapedToHolder)
        {
            //if (!previwPointVFX.gameObject.activeSelf)
            //    previwPointVFX.gameObject.SetActive(true);

            previwPointVFX.position = itemBeingPlaced.transform.position + Vector3.down * .1f;
            previwPointVFX.up = Vector3.up;

            float size = GetItemSize();
            previwPointVFX.localScale = Vector3.one * Mathf.Clamp(size * 0.3f, 0.2f, 2f);
            return;
        }


        // ✅ NEW: handles all wall item cases
        if (IsHoldingWallItem())
        {
            Vector3 wallPos = GetPreviewPosition();

            if (raycaster.HitWall(out _) || raycaster.hasCombinedHit)
            {
                if (!previwPointVFX.gameObject.activeSelf)
                    previwPointVFX.gameObject.SetActive(true);

                previwPointVFX.position = wallPos;
                previwPointVFX.up = itemBeingPlaced.transform.forward;

                float size = GetItemSize();
                previwPointVFX.localScale = Vector3.one * Mathf.Clamp(size * 0.3f, 0.2f, 2f);
            }
            else
            {
                previwPointVFX.gameObject.SetActive(false);
            }
            return;
        }


        Vector3 previewPos = GetPreviewPosition();
        Vector3 rayStart = previewPos + Vector3.up * 0.5f;

        if (raycaster.HitFloor(previewPos, out RaycastHit hit))
        {
            if (!previwPointVFX.gameObject.activeSelf)
                previwPointVFX.gameObject.SetActive(true);

            previwPointVFX.position = hit.point;
            previwPointVFX.up = hit.normal;

            float size = GetItemSize();
            float scale = Mathf.Clamp(size * 0.3f, 0.2f, 2f);
            previwPointVFX.localScale = Vector3.one * scale;
        }
        else
            previwPointVFX.gameObject.SetActive(false);


    }

    private void UpdateLineRendererPositions()
    {
        if (inventory.HasItemInHands() == false)
            return;

        if (itemBeingPlaced == null)
            return;

        lineTopPoint = Vector3.Lerp(lineTopPoint, GetVFXPosition(), Time.deltaTime * 15f);
        lineBottomPoint = Vector3.Lerp(lineBottomPoint, previwPointVFX.transform.position + (Vector3.up * .05f), Time.deltaTime * 15f);

        // Uncomment this if you don't want wiggly lepr for the top point of the line
        //lineRenderer.SetPosition(0, lineBottomPosition);
        lineRenderer.SetPosition(0, previwPointVFX.transform.position + (Vector3.down * .1f));
        lineRenderer.SetPosition(1, lineTopPoint);

        float offset = Time.time * lineRendererSpeed;
        lineRenderer.material.mainTextureOffset = new Vector2(offset, 0f);
    }
    private void EnableLineRendererColor(bool enable)
    {
        if (colorFadeCo != null)
            StopCoroutine(colorFadeCo);


        colorFadeCo = StartCoroutine(EnableLineRendererColorCo(enable));
    }
    private IEnumerator EnableLineRendererColorCo(bool enable)
    {
        if (enable)
        {
            lineRenderer.enabled = true;
            SetLineRendererAlpha(0f);
            yield return new WaitForSeconds(lineRendererFadeInDelay);

            float elapsed = 0f;
            while (elapsed < lineRendererFadeInDuration)
            {
                elapsed += Time.deltaTime;
                SetLineRendererAlpha(Mathf.Clamp01(elapsed / lineRendererFadeInDuration));
                yield return null;
            }

            lineRenderer.colorGradient = lineRendererOriginalColorGradient;
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < lineRendererFadeInDuration)
            {
                elapsed += Time.deltaTime;
                SetLineRendererAlpha(1f - Mathf.Clamp01(elapsed / .1f));
                yield return null;
            }

            SetLineRendererAlpha(0f);
            lineRenderer.enabled = false;
        }

        colorFadeCo = null;
    }
    private void SetLineRendererAlpha(float alpha)
    {
        Gradient g = new Gradient();
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[lineRendererOriginalColorGradient.alphaKeys.Length];

        for (int i = 0; i < alphaKeys.Length; i++)
        {
            float originalAlpha = lineRendererOriginalColorGradient.alphaKeys[i].alpha;
            alphaKeys[i] = new GradientAlphaKey(originalAlpha * alpha, lineRendererOriginalColorGradient.alphaKeys[i].time);
        }

        g.SetKeys(lineRendererOriginalColorGradient.colorKeys, alphaKeys);
        lineRenderer.colorGradient = g;
    }
    private float GetItemSize()
    {
        Renderer[] renderers = itemBeingPlaced.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return 1f;

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        // you can use magnitude or max axis
        return bounds.size.magnitude; // overall size
    }
    private bool IsHoldingWallItem() => itemBeingPlaced.itemData.placementType == PlacementType.WallOnly;

    private IEnumerator SnapItemToHolder(Transform slotTransform)
    {
        yield return StartCoroutine(ArcMoveToVector(
            itemBeingPlaced.transform,
            slotTransform.position,
            0.1f, 0.2f
        ));

        if (itemBeingPlaced == null)
            yield break;

        itemBeingPlaced.EnableAsItWereInHolder(true);
    }

    private IEnumerator ReturnItemToPreview()
    {
     
        itemBeingPlaced.EnableAsItWereInHolder(false);

        yield return StartCoroutine(ArcMoveToVector(
            itemBeingPlaced.transform,
            GetPreviewPosition(),
            0.1f, 0.2f
        ));
    }

    private Vector3 GetVFXPosition()
    {
        if (isSnapedToHolder)
            return itemBeingPlaced.transform.position;

        return GetPreviewPosition();
    }

}
