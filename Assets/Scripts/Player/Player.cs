using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;

    public InputSystem_Actions input { get; private set; }
    public Player_Inventory inventory { get; private set; }
    public Player_Interaction interaction { get; private set; }
    public Player_MoveAndAim moveAndAim { get; private set; }
    public Player_PreviewHandler previewHandler { get; private set; }
    public Player_CameraEffects cameraEffects { get; private set; }
    public Player_Highlighter highlighter { get; private set; }
    public Player_Raycaster raycaster { get; private set; }



    private Quest_MainNPC merchantMain;

    [Header("Courses")]
    [SerializeField] private bool freeCursor;


    private void Awake()
    {
        instance = this;
        input = new InputSystem_Actions();
        input.Enable();

        merchantMain = FindFirstObjectByType<Quest_MainNPC>();
        EnableControlsPlayer();

        input.Player.OpenPauseUI.performed += ctx => UI.instance.OpenPauseUI();
        input.Player.OpenMerchantUI.performed += ctx => UI.instance.OpenShopUI();

        UI.OnEnableInGameUI += EnableControlsPlayer;
        UI.OnEnableUI += EnableControlsUI;

        inventory = GetComponent<Player_Inventory>();
        interaction = GetComponent<Player_Interaction>();
        moveAndAim = GetComponent<Player_MoveAndAim>();
        previewHandler = GetComponent<Player_PreviewHandler>();
        cameraEffects = GetComponent<Player_CameraEffects>();
        highlighter = GetComponent<Player_Highlighter>();
        raycaster = GetComponent<Player_Raycaster>();
    }

  

    public void EnableControlsUI()
    {
        input.Player.Disable();
        input.UI.Enable();

        //if (freeCursor == false)
        //{

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        //}
    }

    public void EnableControlsPlayer()
    {
        input.UI.Disable();
        input.Player.Enable();


        //if (freeCursor == false)
        //{

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        //}
    }
}
