using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class LineOfSightDetector : MonoBehaviour
{
    [SerializeField] float m_detectionHeight;
    [SerializeField] float m_detectionRange;
    [SerializeField] LayerMask m_playerLayerMask;
    Vector3 direction;
    [SerializeField] int FOV;

    [SerializeField] bool showDebugVisuals = true;

    [SerializeField] NavMeshAgent agent;
    [SerializeField] int faceTargetSpeed;
    Vector3 playerDir;
    float angleToPlayer;

    public GameObject performDetection(GameObject potentialTarget)
    {
        RaycastHit hit;
        direction = potentialTarget.transform.position - transform.position;
        angleToPlayer = Vector3.Angle(direction, transform.forward);
        Physics.Raycast(transform.position + Vector3.up * m_detectionHeight,
            direction, out hit, m_detectionRange, m_playerLayerMask);

        if (hit.collider != null && hit.collider.gameObject == potentialTarget && angleToPlayer <= FOV)
        {
            if(showDebugVisuals && this.enabled)
            {
                Debug.DrawLine(transform.position + Vector3.up * m_detectionHeight, potentialTarget.transform.position, Color.red);
            }

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                faceTarget();
            }

            return hit.collider.gameObject;
        }
        else
        {
            return null;
        }
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }
}
