using UnityEngine;
using System.Collections.Generic;

public class MultiplayerCamera : MonoBehaviour
{
    public float smoothSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 5f;
    public float zoomLimiter = 50f;
    private Camera cam;
    private List<Transform> players = new List<Transform>();

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main; // Ensure the script works when attached to an independent GameObject
        
        InvokeRepeating("FindPlayers", 0f, 1f); // Periodically find players
    }

    private void LateUpdate()
    {
        if (players.Count == 0)
            return;

        Move();
        Zoom();
    }

    void FindPlayers()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        players.Clear();
        foreach (GameObject player in playerObjects)
        {
            players.Add(player.transform);
        }
    }

    void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = new Vector3(centerPoint.x, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed);
    }

    void Zoom()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, smoothSpeed);
    }

    float GetGreatestDistance()
    {
        if (players.Count == 1)
            return 0f;

        Bounds bounds = new Bounds(players[0].position, Vector3.zero);
        for (int i = 1; i < players.Count; i++)
        {
            bounds.Encapsulate(players[i].position);
        }
        return bounds.size.x;
    }

    Vector3 GetCenterPoint()
    {
        if (players.Count == 1)
            return players[0].position;

        Bounds bounds = new Bounds(players[0].position, Vector3.zero);
        for (int i = 1; i < players.Count; i++)
        {
            bounds.Encapsulate(players[i].position);
        }
        return bounds.center;
    }
}
