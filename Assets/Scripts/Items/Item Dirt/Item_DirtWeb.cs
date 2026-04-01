using System.Collections;
using UnityEngine;

public class Item_DirtWeb : Item_Dirt
{
    [SerializeField] protected string dissolveProperty = "_Dissolve"; // match your shader property
    [SerializeField] protected Renderer targetRenderer;
    [SerializeField] protected ParticleSystem onCleanFx;
    protected float removeDuration = 0.5f;

    protected Material mat;

    protected override void Awake()
    {
        base.Awake();

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        // Create an instance so we don’t edit shared material
        if (targetRenderer != null)
            mat = targetRenderer.material;
    }
    public void BurnWeb(float interactionDur)
    {
        removeDuration = interactionDur;
        StartCoroutine(CleanDirtCo());
    }


    public override void ShowInputUI(bool enable)
    {
        base.ShowInputUI(enable);

        Item_Tool toolInHand = player.inventory.GetToolInHand();
        bool canBeCleaned = toolInHand != null && toolInHand.CanInteractWith(itemData);

        if (enable)
        {
            string key = canBeCleaned ? "input_web_can_clean" : "input_web_cannot_clean";
            UI.instance.inGameUI.inputHelp.AddInput(KeyType.LMB, key);
        }
        else
            UI.instance.inGameUI.inputHelp.RemoveInput();
    }

    protected override IEnumerator CleanDirtCo()
    {
        if (onCleanFx != null)
            onCleanFx.Play();

        blockOutline = true;
        HideIndicator(true);

        float time = 0f;
        float startDissolve = mat.GetFloat(dissolveProperty);
        float endDissolve = 1f; // fully dissolved

        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one * 0.001f;

        while (time < removeDuration)
        {
            time += Time.deltaTime;
            float t = time / removeDuration;

            // Dissolve
            float dissolveValue = Mathf.Lerp(startDissolve, endDissolve, t);
            mat.SetFloat(dissolveProperty, dissolveValue);

            // Scale shrink
            //transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        mat.SetFloat(dissolveProperty, endDissolve);

        DirtManager.instance.CleanWeb(this);
    }

    public void SetupWeb(DirtDetails webDetails)
    {
        transform.position = webDetails.position;
        transform.rotation = webDetails.rotation;
        meshFilter.mesh = webDetails.mesh;
    }
}
