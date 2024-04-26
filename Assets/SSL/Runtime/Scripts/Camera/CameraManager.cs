using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Camera")]
    [SerializeField] private Camera _camera;

    [Header("Profile System")]
    [SerializeField] private CameraProfile _defaultCameraProfile;
    private CameraProfile _currentCameraProfile;
    private float _profileTransitionTimer = 0f;
    private float _profileTransitionDuration = 0f;
    private Vector3 _profileTransitionStartPosition;
    private float _profileTransitionStartSize;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _InitToDefaultProfile();
    }

    private void Update()
    {
        Vector3 nextPosition = _FindCameraNextPosition();

        if (_IsPlayingProfileTransition())
        {
            _profileTransitionTimer += Time.deltaTime;
            Vector3 transitionPosition = _CalculateProfileTransitionPosition(nextPosition);
            _SetCameraPosition(transitionPosition);
            float transitionSize = _CalculateProfileTransitionCameraSize(_currentCameraProfile.CameraSize);
            _SetCameraSize(transitionSize);
        } else
        {
            _SetCameraPosition(nextPosition);
            _SetCameraSize(_currentCameraProfile.CameraSize);
        }
    }

    private void _InitToDefaultProfile()
    {
        _currentCameraProfile = _defaultCameraProfile;
        _SetCameraPosition(_currentCameraProfile.Position);
        _SetCameraSize(_currentCameraProfile.CameraSize);
    }

    private void _SetCameraPosition(Vector3 position)
    {
        Vector3 newCameraPosition = _camera.transform.position;
        newCameraPosition.x = position.x;
        newCameraPosition.y = position.y;
        _camera.transform.position = newCameraPosition;
    }

    private void _SetCameraSize(float size)
    {
        _camera.orthographicSize = size;
    }

    public void EnterProfile(CameraProfile cameraProfile, CameraProfileTransition transition = null)
    {
        _currentCameraProfile = cameraProfile;
        if(transition != null)
        {
            _PlayProfileTransition(transition);
        }
    }

    public void ExitProfile(CameraProfile cameraProfile, CameraProfileTransition transition = null)
    {
        if (_currentCameraProfile != cameraProfile) return;
        _currentCameraProfile = _defaultCameraProfile;
        if (transition != null)
        {
            _PlayProfileTransition(transition);
        }
        
    }

    private void _PlayProfileTransition(CameraProfileTransition transition)
    {
        _profileTransitionStartPosition = _camera.transform.position;
        _profileTransitionStartSize = _camera.orthographicSize;
        _profileTransitionDuration = transition.duration;
        _profileTransitionTimer = 0f;
    }

    private bool _IsPlayingProfileTransition()
    {
        return _profileTransitionTimer < _profileTransitionDuration;
    }

    private float _CalculateProfileTransitionCameraSize(float enSize)
    {
        float percent = _profileTransitionTimer / _profileTransitionDuration;
        float startSize = _profileTransitionStartSize;
        return Mathf.Lerp(startSize, enSize, percent);
    }

    private Vector3 _CalculateProfileTransitionPosition(Vector3 endPosition)
    {
        float percent = _profileTransitionTimer / _profileTransitionDuration;
        Vector3 origin = _profileTransitionStartPosition;
        return Vector3.Lerp(origin, endPosition, percent);
    }

    private Vector3 _FindCameraNextPosition()
    {
        if(_currentCameraProfile.ProfileType == CameraProfileType.FollowTarget)
        {
            if(_currentCameraProfile.TargetToFollow != null)
            {
                Vector3 destination = _currentCameraProfile.TargetToFollow.position;
                return destination;
            }
        }

        return _currentCameraProfile.Position;

    }
}