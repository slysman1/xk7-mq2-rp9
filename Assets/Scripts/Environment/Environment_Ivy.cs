using UnityEngine;

public enum IvyType { None, First, Second, Third }

public class Environment_Ivy : MonoBehaviour
{
    [SerializeField] private IvyType ivyType;
    [SerializeField] private GameObject[] ivyVariants;

    private void OnValidate()
    {
        UpdateIvy(ivyType);
    }

    public void UpdateIvy(IvyType ivyType)
    {
        this.ivyType = ivyType;


        // Disable all
        foreach (var ivy in ivyVariants)
        {
            if (ivy != null)
                ivy.SetActive(false);
        }

        // Enable selected
        switch (ivyType)
        {
            case IvyType.First:
                if (ivyVariants.Length > 0)
                    ivyVariants[0].SetActive(true);
                break;

            case IvyType.Second:
                if (ivyVariants.Length > 1)
                    ivyVariants[1].SetActive(true);
                break;

            case IvyType.Third:
                if (ivyVariants.Length > 2)
                    ivyVariants[2].SetActive(true);
                break;

            case IvyType.None:
                break;
        }
    }
}