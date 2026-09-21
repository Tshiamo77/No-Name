using UnityEngine;

public class FlashLightToggle : MonoBehaviour
{
    public GameObject flashlight; // Reference to the flashlight GameObject
    private bool isOn = true; // State of the flashlight
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            isOn = !isOn;
            flashlight.SetActive(isOn);
        }
    }
}
