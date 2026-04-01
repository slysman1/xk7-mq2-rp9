using UnityEngine;

public enum WallType { Main, Shelf, Door, WoodstationWall, None }
public enum WallTopType { Main, Window, Cell, Pipe }

[ExecuteAlways] // so it updates in editor too
public class Environment_Wall : MonoBehaviour
{
    private Environment_Ivy ivy;
    private Environment_TopWallWindow window;

    [Header("Current Settings")]
    [SerializeField] private WallType wallType;
    [SerializeField] private WallTopType wallTopType;
    [SerializeField] private WindowType windowType;
    [SerializeField] private IvyType ivyType;

    [Header("Wall Setup")]
    [SerializeField] private GameObject mainWall;
    [SerializeField] private GameObject shelfWall;
    [SerializeField] private GameObject doorWall;
    [SerializeField] private GameObject woodstationWall;
    [Space]
    [SerializeField] private GameObject[] allWallsVariants;

    [Header("Top Wall Setup")]
    [SerializeField] private GameObject mainTopWall;
    [SerializeField] private GameObject windowTopWall;
    [SerializeField] private GameObject cellTopWall;
    [SerializeField] private GameObject pipeTopWall;
    [Space]
    [SerializeField] private GameObject[] topWallVariants;



    private void OnValidate()
    {
        if(ivy == null)
            ivy = GetComponentInChildren<Environment_Ivy>(true);

        if(window == null)
            window = GetComponentInChildren<Environment_TopWallWindow>(true);

        UpdateWall();
        UpdateTopWall();

        ivy.UpdateIvy(ivyType);
        window.UpdateWindow(windowType);
    }

    public void UpdateWall()
    {
        foreach (var variant in allWallsVariants)
            variant.SetActive(false);

        switch (wallType)
        {
            case WallType.Main:
                mainWall.SetActive(true);
                break;
            case WallType.Shelf:
                shelfWall.SetActive(true);
                break;
            case WallType.Door:
                doorWall.SetActive(true);
                break;
            case WallType.WoodstationWall:
                woodstationWall.SetActive(true);
                break;
            case WallType.None:
                break;
        }
    }

    public void UpdateTopWall()
    {
        foreach (var variant in topWallVariants)
            variant.SetActive(false);

        switch (wallTopType)
        {
            case WallTopType.Main:
                mainTopWall.SetActive(true);
                break;

            case WallTopType.Window:
                windowTopWall.SetActive(true);
                break;
               

            case WallTopType.Cell:
                cellTopWall.SetActive(true);
                break;

            case WallTopType.Pipe:
                pipeTopWall.SetActive(true);
                break;
        }
    }
}
