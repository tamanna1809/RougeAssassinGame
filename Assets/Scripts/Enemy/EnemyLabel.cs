using UnityEngine;
using TMPro;

public class EnemyLabel : MonoBehaviour
{
    public string designation = "TARGET_ALPHA";
    public TextMeshPro labelText;

    void Start()
    {
        if (labelText != null)
        {
            labelText.text = designation;
        }
    }

    void LateUpdate()
    {
        // Keep label upright regardless of enemy rotation
        if (labelText != null)
        {
            labelText.transform.rotation = Quaternion.identity;
        }
    }
}
