using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class IKMovementV2 : MonoBehaviour
{
    private float _inputX;
    private float _inputZ;
    private Vector3 _moveDirection;
    private bool _blockRotationPlayer = false;
    public Animator _anim;
    private float _speed;
    [Range(3f, 8f)] public float desiredLinearSpeed;
    [Range(0f, 0.5f)] public float desiredRotationSpeed;
    [Range(0, 1f)] public float allowPlayerRotation = 0.1f;
    private CharacterController _characterController;
    private Rigidbody _rigidbody;
    private CapsuleCollider _collider; 
    private float _initialCapsuleColliderHeight;

    private Vector3 _rightFootPos, _leftFootPos, _rightFootIKPos, _leftFootIKPos;
    private Quaternion _rightFootIKRotation, _leftFootIKRotation;
    private float _lastPelvisPositionY, _lastRightFootPosY, _lastLeftFootPosY;

    [Header("Feet Grounder")]
    public bool _enableFeetIK = true;
    [Range(0, 2f)] [SerializeField] private float _heightFromGroundRaycast = 1.14f;
    [Range(0, 2f)] [SerializeField] private float _raycastDownDistance = 1.5f;
    [SerializeField] private LayerMask _environnmentLayer;
    [SerializeField] private float _pelvisOffset = 0f;
    [Range(0, 1f)] [SerializeField] private float _pelvisUpDownSpeed = 0.28f;
    [Range(0, 1f)] [SerializeField] private float _feetToIKPosSpeed = 0.5f;

    public string leftFootAnimVariableName = "LeftFootCurve";
    public string rightFootAnimVariableName = "RightFootCurve";

    public bool _useProIKFeature = false;
    public bool _showSolverDebug = true;



    [Header("AnimationSmoothing")]
    [Range(0, 1f)] public float HorizontalAnimSmoothTime = 0.2f;
    [Range(0, 1f)] public float VerticalAnimTime = 0.2f;
    [Range(0, 1f)] public float StartAnimTime = 0.2f;
    [Range(0, 1f)] public float StopAnimTime = 0.2f;

    private float _verticalVel; //
    private Vector3 _moveVector; //

    private void Start()
    {
        _anim = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
        _initialCapsuleColliderHeight = _collider.center.y;
    }

    private void Update()
    {
        InputMagnitude();
    }

    #region PlayerMovement

    private void PlayerMoveAndRotation()
    {
        _inputX = Input.GetAxis("Horizontal");
        _inputZ = Input.GetAxis("Vertical");

        _moveDirection = transform.forward * _inputZ + transform.right * _inputX;
        if (_blockRotationPlayer == false)
        {
            //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(transform.right * _inputX), desiredRotationSpeed);
        }

        _rigidbody.MovePosition(transform.position + transform.forward * _inputZ * Time.deltaTime * desiredLinearSpeed);

        Vector3 rotation = _inputX * transform.up * desiredRotationSpeed * Time.deltaTime;
        Quaternion deltaRotation = Quaternion.Euler(rotation);
        _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);
    }

    private void InputMagnitude()
    {
        _inputX = Input.GetAxis("Horizontal");
        _inputZ = Input.GetAxis("Vertical");
        float dt = Time.deltaTime;

        _anim.SetFloat("InputX", _inputX, HorizontalAnimSmoothTime, dt * 2f);
        _anim.SetFloat("InputZ", _inputZ, VerticalAnimTime, dt * 2f);

        _speed = new Vector2( _inputX, _inputZ ).sqrMagnitude;

        if (_speed > allowPlayerRotation)
        {
            _anim.SetFloat("InputMagnitude", _speed, StartAnimTime, dt);
            PlayerMoveAndRotation();
        }
        else if (_speed < allowPlayerRotation)
        {
            _anim.SetFloat("InputMagnitude", _speed, StopAnimTime, dt);
        }

        //PlayerMoveAndRotation();
    }

    #endregion


    #region FeetGrounding

    private void FixedUpdate()
    {
        if (_enableFeetIK == false) return;
        if (_anim == null) return;

        AdjustFeetTarget(ref _rightFootPos, HumanBodyBones.RightFoot);
        AdjustFeetTarget(ref _leftFootPos, HumanBodyBones.LeftFoot);

        // Raycast to the ground to find positions
        FeetPositionSolver(_rightFootPos, ref _rightFootIKPos, ref _rightFootIKRotation);
        FeetPositionSolver(_leftFootPos, ref _leftFootIKPos, ref _leftFootIKRotation);
    }

    private void OnAnimatorIK( int layerIndex )
    {
        if (_enableFeetIK == false) return;
        if (_anim == null) return;

        MovePelvisHeight();

        // Right Foot Position and Rotation
        _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        if (_useProIKFeature)
        {
            _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, _anim.GetFloat(rightFootAnimVariableName));
        }
        MoveFeetToIKPoint(AvatarIKGoal.RightFoot, _rightFootIKPos, _rightFootIKRotation, ref _lastRightFootPosY);

        // Left Foot Position and Rotation
        _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        if (_useProIKFeature)
        {
            _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _anim.GetFloat(leftFootAnimVariableName));
        }
        MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, _leftFootIKPos, _leftFootIKRotation, ref _lastLeftFootPosY);
    }

    #endregion

    #region FeetGroundingMethods

    private void MoveFeetToIKPoint ( AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY )
    {
        Vector3  targetIKPosition = _anim.GetIKPosition(foot);

        if ( positionIKHolder != Vector3.zero )
        {
            targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
            positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

            float yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, _feetToIKPosSpeed);
            targetIKPosition.y += yVariable;

            lastFootPositionY = yVariable;

            targetIKPosition = transform.TransformPoint(targetIKPosition);

            _anim.SetIKRotation(foot, rotationIKHolder);
        }

        _anim.SetIKPosition(foot, targetIKPosition);
    }

    private void MovePelvisHeight()
    {
        if (_rightFootIKPos == Vector3.zero ||  _leftFootIKPos == Vector3.zero || _lastPelvisPositionY == 0)
        {
            //_lastPelvisPositionY = _anim.bodyPosition.y;
            _lastPelvisPositionY = transform.position.y;
            return;
        }

        float leftOffsetPos = _leftFootIKPos.y - transform.position.y;
        float rightOffsetPos = _rightFootIKPos.y - transform.position.y;
        float totalOffset = (leftOffsetPos < rightOffsetPos) ? leftOffsetPos : rightOffsetPos;

       // Vector3 newPelvisPos = _anim.bodyPosition + Vector3.up * totalOffset;
        Vector3 newPelvisPos = transform.position + Vector3.up * totalOffset;

        newPelvisPos.y  = Mathf.Lerp(_lastPelvisPositionY, newPelvisPos.y, _pelvisUpDownSpeed);

        //_anim.bodyPosition = newPelvisPos;
        transform.position = newPelvisPos;
        _lastPelvisPositionY = newPelvisPos.y;
        _collider.center = Vector3.up * (transform.position.y + _initialCapsuleColliderHeight);
    }

    private void FeetPositionSolver ( Vector3 fromSkyPosition, ref Vector3 feetIKPositions, ref Quaternion feetIKRotations )
    {
        RaycastHit feetOutHit;

        if (_showSolverDebug)
        {
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (_raycastDownDistance + _heightFromGroundRaycast), Color.yellow);
        }

        if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, _raycastDownDistance + _heightFromGroundRaycast, _environnmentLayer))
        {
            feetIKPositions = fromSkyPosition;
            feetIKPositions.y = feetOutHit.point.y + _pelvisOffset;
            feetIKRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;
            return;
        }

        // Hit didn't work
        feetIKPositions = Vector3.zero;
    }

    private void AdjustFeetTarget ( ref Vector3 feetPositions, HumanBodyBones foot)
    {
        feetPositions = _anim.GetBoneTransform(foot).position;
        feetPositions.y = transform.position.y + _heightFromGroundRaycast;
    }

    #endregion
}
