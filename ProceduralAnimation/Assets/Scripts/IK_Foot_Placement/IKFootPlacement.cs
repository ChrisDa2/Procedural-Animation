using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class IKFootPlacement : MonoBehaviour
{
    private Animator _anim;
    public float _speedUp;
    public Vector3 TargetPos;

    public float leftFootY;
    public float rightFootY;
    public float initialFootHeight;
    public float feetOffset;
    public float yTarget;
    public float lerpSpeedUp;
    public float lerpSpeedDown;
    public float forwardMultiplier;
    [Range(1f, 5f)] public float downMultiplier;
    public float verticalThreshold;
    public float feetBaseDiff;

    public LayerMask LayerMask;

    [Range(0f, 1f)] public float DistanceToGround;

    [Range(1f, 1.5f)] public float multiplierUpDown;
    public bool firstApproach;
    public bool secondApproach;

    

    private void Start()
    {
        _anim = GetComponent<Animator>();
        TargetPos = transform.position;
        initialFootHeight = _anim.GetBoneTransform(HumanBodyBones.LeftFoot).position.y;
    }

    private void Update()
    {
        if (firstApproach)
        {
            // Pelvis
            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hit, DistanceToGround + 2f, LayerMask))
            {
                yTarget = hit.point.y - transform.position.y;
                if (hit.transform.tag == "Walkable" && Mathf.Abs(yTarget) > 0.08f)
                {
                    TargetPos = transform.position + (Vector3.up * yTarget * multiplierUpDown) + (transform.forward * 0f);
                    transform.position = Vector3.MoveTowards(transform.position, TargetPos, _speedUp * Time.deltaTime);
                }
            }
        }
        else if (secondApproach)
        {
            yTarget = (leftFootY < rightFootY) ? leftFootY : rightFootY;
            yTarget = yTarget - initialFootHeight;
            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hit, DistanceToGround + 3f, LayerMask))
            {
                yTarget = (yTarget < hit.point.y) ? yTarget : (hit.point.y - transform.position.y);
                feetBaseDiff = hit.point.y - yTarget;
                if (Mathf.Abs(yTarget - transform.position.y) > verticalThreshold)
                {
                    TargetPos = transform.position + (Vector3.up * yTarget) + (transform.forward * Time.deltaTime * Input.GetAxis("Vertical") * forwardMultiplier);
                    if (TargetPos.y > transform.position.y)
                    {
                        transform.position = Vector3.Lerp(transform.position, TargetPos, lerpSpeedUp * Time.deltaTime);
                    }
                    else
                    {
                        transform.position = Vector3.Lerp(transform.position, TargetPos, lerpSpeedDown * Time.deltaTime);
                    }
                }
            }
        }
        else
        {

            feetOffset = Mathf.Abs(leftFootY - rightFootY);
            yTarget = (leftFootY < rightFootY) ? leftFootY : rightFootY;
            yTarget = yTarget - initialFootHeight;
            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hit, DistanceToGround + 3f, LayerMask))
            {
                //yTarget = (yTarget < hit.point.y) ? yTarget : (hit.point.y - yTarget);
                if (Mathf.Abs(yTarget - transform.position.y) > verticalThreshold)
                {
                    TargetPos = transform.position + (Vector3.up * yTarget) + (transform.forward * Time.deltaTime * forwardMultiplier);
                    transform.position = Vector3.Lerp(transform.position, TargetPos, lerpSpeedUp * Time.deltaTime);
                }
                else if (Mathf.Abs(yTarget - transform.position.y) < verticalThreshold && Input.GetAxis("Vertical") == 0)
                {
                    TargetPos = transform.position + (Vector3.up * yTarget);
                    transform.position = Vector3.Lerp(transform.position, TargetPos, lerpSpeedUp * Time.deltaTime);
                }
            }
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
        _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);

        RaycastHit hit;

        // Left Foot
        Ray ray = new Ray(_anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
        if (Physics.Raycast(_anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down, out hit, DistanceToGround + 1f, LayerMask))
        {
            if (hit.transform.tag == "Walkable")
            {
                Vector3 footPosition = hit.point;
                footPosition.y += DistanceToGround;
                _anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                _anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
            }
        }

        // Right Foot
        ray = new Ray(_anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
        if (Physics.Raycast(ray, out hit, DistanceToGround + 1f, LayerMask))
        {
            if (hit.transform.tag == "Walkable")
            {
                Vector3 footPosition = hit.point;
                footPosition.y += DistanceToGround;
                _anim.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
                _anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
            }
        }

        // Update positions
        leftFootY = _anim.GetIKPosition(AvatarIKGoal.LeftFoot).y;
        rightFootY = _anim.GetIKPosition(AvatarIKGoal.RightFoot).y;
    }
}
