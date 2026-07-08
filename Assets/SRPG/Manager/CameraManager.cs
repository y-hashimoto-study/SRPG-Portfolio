using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    [SerializeField] private CinemachineCamera _cinemachine;
    [SerializeField] private float _moveSpeed = 15f;
    public int CurrentZoom = 0;
    private CinemachineFollow _cinemachineFollow;
    private PlayerInputActions _inputAction;
    private bool _canMoveCamera = true;
    [SerializeField] private int _minX = 0;
    [SerializeField] private int _maxX = 90;
    [SerializeField] private int _minZ = 0;
    [SerializeField] private int _maxZ = 90;
    [SerializeField] private int _defaultHeight = 60;
    [SerializeField] private int _zoomStep = 10;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _inputAction = new PlayerInputActions();
    }
    void OnEnable()
    {
        _inputAction.Enable();
        _inputAction.Player.CameraZoom.performed += ReadZoom;
    }
    void OnDisable()
    {
        _inputAction.Player.CameraZoom.performed -= ReadZoom;
        _inputAction.Disable();
    }
    void Start()
    {
        _cinemachineFollow = _cinemachine.GetComponent<CinemachineFollow>();
    }
    void Update()
    {
        if(_inputAction == null || !_canMoveCamera)return;
        Vector2 input = _inputAction.Player.CameraMove.ReadValue<Vector2>();
        if(input.sqrMagnitude > 0)
        {
            NonTargetCamera();
            Vector3 moveCamera = new Vector3 (input.x,0,input.y) * _moveSpeed * Time.deltaTime;
            float moveX = Mathf.Clamp(_cinemachine.transform.position.x + moveCamera.x,_minX,_maxX);
            float moveZ = Mathf.Clamp(_cinemachine.transform.position.z + moveCamera.z,_minZ,_maxZ);
            _cinemachine.transform.position = new Vector3(moveX,_cinemachine.transform.position.y,moveZ);
        }
    }
    private void ReadZoom(InputAction.CallbackContext context)
    {
        Vector2 scroll = context.ReadValue<Vector2>();
        if(scroll.y > 0f) Zoom(true);
        if(scroll.y < 0f) Zoom(false);
    }
    public void TargetCamera(Transform targetTransform)
    {
        _cinemachine.transform.position = new Vector3(targetTransform.position.x,_cinemachine.transform.position.y,targetTransform.position.z);
        _cinemachine.Target.TrackingTarget = targetTransform;
    }
    public void NonTargetCamera()
    {
        _cinemachine.Target.TrackingTarget = null;
    }
    public void Zoom(bool isZoom)
    {
        CurrentZoom += isZoom ? 1:-1;
        CurrentZoom = Mathf.Clamp(CurrentZoom,0,2);
        _cinemachineFollow.FollowOffset = new Vector3(0,_defaultHeight - CurrentZoom * _zoomStep ,0);
        if(_cinemachine.Target.TrackingTarget == null)
        {
            Vector3 currentPosition = _cinemachine.transform.position;
            _cinemachine.transform.position = new Vector3(currentPosition.x,_defaultHeight - CurrentZoom * _zoomStep,currentPosition.z);
        }
    }
    public void CanMoveCamera(bool lockcamera)
    {
        _canMoveCamera = lockcamera;
    }
}
