using UnityEngine;

public class EnableDisablePrefab : MonoBehaviour
{
    public GameObject prefabToToggle;

    public void TogglePrefab()
    {
        if (prefabToToggle != null)
        {
            prefabToToggle.SetActive(!prefabToToggle.activeSelf);
        }
    }
}
