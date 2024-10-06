using UnityEngine;
using Niantic.Lightship.AR.NavigationMesh;
using UnityEngine.InputSystem;
using System.Linq;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField]
    private LightshipNavMeshManager _lightshipNavMeshManager;

    [SerializeField]
    private GameObject _objectToPlace;

    private float _lastClickTime;
    private float _doubleClickThreshold = 0.3f; // Maximum time between double clicks
    private bool _isTreatPlacementEnabled;

    // Called to toggle treat placement
    public void ToggleTreatPlacement()
    {
        _isTreatPlacementEnabled = !_isTreatPlacementEnabled;
        Debug.Log("Treat placement toggled: " + _isTreatPlacementEnabled);
    }

    private void Update()
    {
        if (!_isTreatPlacementEnabled)
        {
            return;
        }

        // Detect input for double-click or double-tap
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Debug.Log("Pointer detected, checking for double-click...");

            HandleInput();
        }
        else
        {
            Debug.Log("Pointer.current or press is null.");
        }
    }

    private void HandleInput()
    {
        float timeSinceLastClick = Time.time - _lastClickTime;

        if (timeSinceLastClick < _doubleClickThreshold)
        {
            Debug.Log("Double-click detected!");

            Vector2 screenPosition = Pointer.current.position.ReadValue();
            Debug.Log("Screen position: " + screenPosition);

            // Cast ray from pointer position
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Raycast hit: " + hit.collider.name + " at " + hit.point);
                PlaceObjectAtPosition(hit.point);
            }
            else
            {
                Debug.Log("Raycast didn't hit anything.");
            }
        }

        _lastClickTime = Time.time;
    }

    private void PlaceObjectAtPosition(Vector3 hitPosition)
    {
        var surfaces = _lightshipNavMeshManager.LightshipNavMesh.Surfaces;

        Debug.Log("Number of surfaces: " + surfaces.Count);

        foreach (var surface in surfaces)
        {
            foreach (var element in surface.Elements)
            {
                Vector3 position = NavMeshUtils.TileToPosition(
                    element.Coordinates,
                    surface.Elevation,
                    _lightshipNavMeshManager.LightshipNavMesh.Settings.TileSize
                );

                Debug.Log("Checking position: " + position);

                if (Vector3.Distance(position, hitPosition) < _lightshipNavMeshManager.LightshipNavMesh.Settings.TileSize)
                {
                    position.y += 0.5f;

                    Debug.Log("Placing treat at position: " + position);
                    Instantiate(_objectToPlace, position, Quaternion.identity);
                    return;
                }
            }
        }

        Debug.Log("No valid navmesh tile found near the hit point.");
    }

    public static class NavMeshUtils
    {
        public static Vector3 TileToPosition(Vector2Int tileCoordinates, float elevation, float tileSize)
        {
            float x = tileCoordinates.x * tileSize;
            float z = tileCoordinates.y * tileSize;
            float y = elevation;

            return new Vector3(x, y, z);
        }
    }
}
