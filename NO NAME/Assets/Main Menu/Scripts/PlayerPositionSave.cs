using UnityEngine;

public class PlayerPositionSave : MonoBehaviour
{
    private const string PosX = "NoName_PlayerPositionX";
    private const string PosY = "NoName_PlayerPositionY";
    private const string PosZ = "NoName_PlayerPositionZ";
    private const string RotY = "NoName_PlayerRotationY";

    private CharacterController characterController;
    private float saveTimer;
    private const float SaveInterval = 2f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        if (!MainMenuController.LoadSavedPosition)
            return;

        if (!PlayerPrefs.HasKey(PosX) ||
            !PlayerPrefs.HasKey(PosY) ||
            !PlayerPrefs.HasKey(PosZ))
            return;

        Vector3 savedPosition = new Vector3(
            PlayerPrefs.GetFloat(PosX),
            PlayerPrefs.GetFloat(PosY),
            PlayerPrefs.GetFloat(PosZ)
        );

        if (characterController != null)
            characterController.enabled = false;

        transform.position = savedPosition;

        if (characterController != null)
            characterController.enabled = true;

        if (PlayerPrefs.HasKey(RotY))
        {
            Vector3 angles = transform.eulerAngles;
            angles.y = PlayerPrefs.GetFloat(RotY);
            transform.eulerAngles = angles;
        }

        MainMenuController.LoadSavedPosition = false;
    }

    private void Update()
    {
        saveTimer += Time.deltaTime;

        if (saveTimer >= SaveInterval)
        {
            SavePosition();
            saveTimer = 0f;
        }
    }

    public void SavePosition()
    {
        PlayerPrefs.SetFloat(PosX, transform.position.x);
        PlayerPrefs.SetFloat(PosY, transform.position.y);
        PlayerPrefs.SetFloat(PosZ, transform.position.z);
        PlayerPrefs.SetFloat(RotY, transform.eulerAngles.y);
        PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            SavePosition();
    }

    private void OnApplicationQuit()
    {
        SavePosition();
    }
}