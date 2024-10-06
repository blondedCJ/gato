using UnityEngine;
using Niantic.Lightship.AR.NavigationMesh;
using UnityEngine.InputSystem;

public class NavMeshHowTo : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private LightshipNavMeshManager _navmeshManager;

    [SerializeField]
    private LightshipNavMeshAgent _agentPrefab;

    private LightshipNavMeshAgent _agentInstance;

    private float _lastClickTime = 0f;
    private float _doubleClickTime = 0.2f; // Time allowed between double-clicks/taps

    // Define input actions for the new input system
    private InputAction _touchInputAction;

    private void Awake()
    {
        // Initialize the InputAction for touch or click
        _touchInputAction = new InputAction(type: InputActionType.PassThrough, binding: "<Pointer>/press");
        _touchInputAction.performed += ctx => HandleTouch(ctx);
        _touchInputAction.Enable();
    }

    private void OnDestroy()
    {
        // Clean up the input action when the object is destroyed
        _touchInputAction.performed -= ctx => HandleTouch(ctx);
        _touchInputAction.Disable();
    }

    void Update()
    {
        // The new input system handles input in the action callbacks, so no need to call HandleTouch here.
    }

    public void SetVisualization(bool isVisualizationOn)
    {
        // Turn off the rendering for the navmesh
        _navmeshManager.GetComponent<LightshipNavMeshRenderer>().enabled = isVisualizationOn;

        if (_agentInstance != null)
        {
            // Turn off the path rendering on the active agent
            _agentInstance.GetComponent<LightshipNavMeshAgentPathRenderer>().enabled = isVisualizationOn;
        }
    }

    private void HandleTouch(InputAction.CallbackContext context)
    {
        // Check if input is a click/tap
        if (!context.performed)
            return;

        float timeSinceLastClick = Time.time - _lastClickTime;

        if (timeSinceLastClick <= _doubleClickTime)
        {
            Ray ray;
#if UNITY_EDITOR
            ray = _camera.ScreenPointToRay(Pointer.current.position.ReadValue());
#else
            ray = _camera.ScreenPointToRay(Touchscreen.current.primaryTouch.position.ReadValue());
#endif

            // Project the touch point from screen space into 3d and pass that to your agent as a destination
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (_agentInstance == null)
                {
                    _agentInstance = Instantiate(_agentPrefab);
                    _agentInstance.transform.position = hit.point;
                }
                else
                {
                  // _agentInstance.SetDestination(hit.point);
                }
            }
        }

        // Update the last click time
        _lastClickTime = Time.time;
    }
}
