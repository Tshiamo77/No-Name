using UnityEngine;
using UnityEngine.SceneManagement;

public static class KeyManager
{
    public static bool HasKey { get; private set; }
    public static GameObject HeldKeyObject { get; private set; }

    public static void PickUpKey(GameObject keyObject)
    {
        HasKey = true;
        HeldKeyObject = keyObject;
    }

    public static void UseKey()
    {
        HasKey = false;
        if (HeldKeyObject != null)
        {
            Object.Destroy(HeldKeyObject);
        }
        HeldKeyObject = null;
    }

    public static void Reset()
    {
        HasKey = false;
        HeldKeyObject = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        Reset();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Reset();
    }
}