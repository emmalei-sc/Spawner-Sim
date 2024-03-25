using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TopDownCamera : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private float distance;
    [SerializeField] private float pitch;
    [SerializeField] private float yaw;
    [SerializeField] private GameObject target;

    [Header("Camera Movement")]
    [SerializeField] AnimationCurve panSpeedByDistance;
    [SerializeField] AnimationCurve zoomSpeedCurve;

    [SerializeField] private float defaultHeight;
    [SerializeField] private float MaxCameraHeight;
    [SerializeField] private float MinCameraHeight;

    private Camera camComp;
    private float zoomSpeed = 2.0f;
    private float panSpeed = 0.2f;
    private Vector3 lastMousePosition;

    private void Awake()
    {
        camComp = GetComponent<Camera>();
    }

    void Start()
    {
        if (!camComp.orthographic)
            Debug.LogWarning("TopDownCamera is not attached to an orthographic camera");
    }
    private void OnEnable()
    {
        ResetCamera();
    }

    void Update()
    {
        HandleZoom();
        HandlePanMouse();
        HandlePanWASD();

        //Reset Main Camera Position
        if (Input.GetKey(KeyCode.R))
            ResetCamera();
    }

    private void HandleZoom()
    {
        // Scroll to zoom
        float zoomProg = (camComp.orthographicSize - MinCameraHeight) / MaxCameraHeight;
        zoomSpeed = zoomSpeedCurve.Evaluate(zoomProg);

        float zoom = -Input.mouseScrollDelta.y * zoomSpeed;
        float newSize = camComp.orthographicSize + zoom;
        camComp.orthographicSize = Mathf.Clamp(newSize, MinCameraHeight, MaxCameraHeight);
    }

    private void HandlePanMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = camComp.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 diff = lastMousePosition - camComp.ScreenToWorldPoint(Input.mousePosition);
            transform.position += diff;
        }
    }

    private void HandlePanWASD()
    {
        float zoomProg = (camComp.orthographicSize - MinCameraHeight) / MaxCameraHeight;
        panSpeed = panSpeedByDistance.Evaluate(zoomProg);
        Vector3 dir = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            dir = transform.up;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            dir = transform.up * -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            dir = transform.right;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            dir = transform.right * -1f;

        dir.y = 0f;
        transform.position += dir * panSpeed * Time.deltaTime;
    }

    private void ResetCamera()
    {
        camComp.orthographicSize = defaultHeight;
        CenterCamera();
    }

    private void CenterCamera()
    {
        //Calculate how far away to be in each dimension
        Vector3 offset = Vector3.back * distance;
        Matrix4x4 pitchTransform = Matrix4x4.Rotate(Quaternion.AngleAxis(pitch, Vector3.right));
        offset = pitchTransform.MultiplyPoint(offset);
        Matrix4x4 yawTransform = Matrix4x4.Rotate(Quaternion.AngleAxis(yaw, Vector3.up));
        offset = yawTransform.MultiplyPoint(offset);

        //Move and rotate
        Vector3 goalPos = target.gameObject.transform.position + offset;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = goalPos;
    }
}