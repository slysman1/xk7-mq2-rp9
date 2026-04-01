using UnityEngine;

public class TestingManager : MonoBehaviour
{
    public static TestingManager instance;

    public bool noNeedUpgrade;
    public bool ignoreTutorial;

    private void Awake()
    {
        instance = this;
    }
}
