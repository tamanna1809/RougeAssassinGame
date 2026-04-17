using UnityEngine;

public class MinimapController : MonoBehaviour
{
    [Header("Minimap Camera")]
    public Camera minimapCamera;
    public float cameraSize = 20f;

    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = cameraSize;
        }
    }

    void LateUpdate()
    {
        if (player == null || minimapCamera == null) return;

        // Follow player
        Vector3 pos = player.position;
        pos.z = minimapCamera.transform.position.z;
        minimapCamera.transform.position = pos;
    }
}
