using UnityEngine;

public class ProceduralMonsterWalk : MonoBehaviour
{
    public Transform body;
    public Transform frontLeftLeg;
    public Transform frontRightLeg;
    public Transform backLeftLeg;
    public Transform backRightLeg;

    public float stepHeight = 0.2f;
    public float stepDistance = 0.5f;
    public float speed = 1.0f;
    public float bodyHeightAdjustment = 0.1f;
    public float legMoveThreshold = 0.5f; // Distance that triggers leg movement
    public LayerMask groundLayer; // LayerMask to define what is considered ground

    private Vector3 previousBodyPosition;
    private Vector3 frontLeftLegTarget;
    private Vector3 frontRightLegTarget;
    private Vector3 backLeftLegTarget;
    private Vector3 backRightLegTarget;

    void Start()
    {
        previousBodyPosition = body.position;
        frontLeftLegTarget = frontLeftLeg.position;
        frontRightLegTarget = frontRightLeg.position;
        backLeftLegTarget = backLeftLeg.position;
        backRightLegTarget = backRightLeg.position;
    }

    void Update()
    {
        Vector3 bodyMovement = body.position - previousBodyPosition;

        // Move legs if body has moved enough
        if (bodyMovement.magnitude > legMoveThreshold)
        {
            MoveLeg(frontLeftLeg, ref frontLeftLegTarget);
            MoveLeg(frontRightLeg, ref frontRightLegTarget);
            MoveLeg(backLeftLeg, ref backLeftLegTarget);
            MoveLeg(backRightLeg, ref backRightLegTarget);

            previousBodyPosition = body.position;
        }

        // Apply the targets to the legs
        ApplyLegOffset(frontLeftLeg, frontLeftLegTarget);
        ApplyLegOffset(frontRightLeg, frontRightLegTarget);
        ApplyLegOffset(backLeftLeg, backLeftLegTarget);
        ApplyLegOffset(backRightLeg, backRightLegTarget);

        // Adjust body height based on leg positions
        AdjustBodyPosition();
    }

    void MoveLeg(Transform leg, ref Vector3 legTarget)
    {
        RaycastHit hit;
        Vector3 rayOrigin = leg.position + Vector3.up * 1.0f; // Adjust the height of the ray origin if necessary
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            legTarget = hit.point + Vector3.up * stepHeight; // Adjust the leg target height if necessary
        }
    }

    void ApplyLegOffset(Transform leg, Vector3 targetPosition)
    {
        if (leg != null)
        {
            leg.position = Vector3.Lerp(leg.position, targetPosition, Time.deltaTime * speed);
        }
    }

    void AdjustBodyPosition()
    {
        if (body != null)
        {
            float avgY = (frontLeftLeg.position.y + frontRightLeg.position.y + backLeftLeg.position.y + backRightLeg.position.y) / 4;
            body.position = new Vector3(body.position.x, avgY + bodyHeightAdjustment, body.position.z);
        }
    }
}
