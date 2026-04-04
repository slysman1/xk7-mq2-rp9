using UnityEngine;

public class UI_OnObjectIndicator : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private GameObject indicator;
    [SerializeField] private float defaultOffset = 1f;
    [SerializeField] private float savedOffset = 1f;

    [Header("Rotation")]
    [Tooltip("Default Z rotation applied on top of facing the player")]
    [SerializeField] private float defaultZRotation = 0f;

    [Header("Bobbing")]
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobSpeed = 2f;

    private bool isHidden;
    private RectTransform myRect;
    private Transform cam;
    private Vector3 baseOffset;
    private float defaultScale;


    private void Awake()
    {
        myRect = GetComponent<RectTransform>();
        cam = Camera.main.transform;
        defaultScale = myRect.localScale.x;
        savedOffset = defaultOffset;
    }

    private void LateUpdate()
    {
        if (!indicator.activeSelf)
            return;

        UpdatePosition();
        UpdateRotation();
    }

    public void HideIndicator(bool hide)
    {
        isHidden = hide;
        indicator.SetActive(!isHidden);
    }

    public void AttachTo(Transform newTarget,bool hasPositioner = false)
    {
        HideIndicator(false);
        transform.SetParent(newTarget, false);
        myRect.localPosition = Vector3.zero;

        savedOffset = hasPositioner ? 0 : defaultOffset;
        EnableIndicator(true);
        myRect.localScale = Vector3.one * defaultScale;
    }

    public void Detach(Transform newParent)
    {
        transform.SetParent(newParent);
        EnableIndicator(false);
    }


    public void EnableIndicator(bool enable)
    {
        if(isHidden)
            return;
        
        indicator.SetActive(enable);

        if (enable)
        {

            // Reset / re-setup every time it's enabled
            baseOffset = Vector3.up * savedOffset;
            UpdatePosition();
            UpdateRotation();
        }
    }

    private void UpdatePosition()
    {
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        indicator.transform.position =
            transform.position + baseOffset + Vector3.up * bob;
    }
    private void UpdateRotation()
    {
        Vector3 lookDir = cam.position - indicator.transform.position;
        lookDir.y = 0f; // Y-only facing

        if (lookDir.sqrMagnitude < 0.001f)
            return;

        Quaternion lookRot = Quaternion.LookRotation(lookDir);
        Quaternion zOffset = Quaternion.Euler(0f, 0f, defaultZRotation);

        indicator.transform.rotation = lookRot * zOffset;
    }
}
