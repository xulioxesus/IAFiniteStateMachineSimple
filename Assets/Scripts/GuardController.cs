using UnityEngine;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class GuardController : MonoBehaviour
{
    // FSM
    enum State { Patrol, Investigate, Chase };
    State curState = State.Patrol;

    // Last place the player was seen
    Vector3 lastPlaceSeen;
    public Transform player;
    float fovDistance = 20.0f;
    float fovAngle = 45.0f;

    // Debug/Visualization
    [Header("Debug/Visualization")]
    public bool showFOVGizmos = true;

    // Chasing settings

    public float chasingSpeed = 2.0f;
    public float chasingRotSpeed = 2.0f;
    public float chasingAccuracy = 5.0f;

    // Patrol settings
    public float patrolDistance = 10.0f;
    float patrolWait = 5.0f;
    float patrolTimePassed = 0;

    public float knockRadius = 20.0f;

    bool ICanSee(Transform player)
    {
        Vector3 direction = player.position - this.transform.position;
        float angle = Vector3.Angle(direction, this.transform.forward);

        RaycastHit hit;
        if (
            Physics.Raycast(this.transform.position, direction, out hit) && // Can I cast a ray from my position to the player's position?
            hit.collider.gameObject.tag == "Player" && // Did the ray hit the player?
            direction.magnitude < fovDistance && // Is the player close enough to be seen?
            angle < fovAngle // Is the player in the view cone?
        )
        {
            return true;
        }
        return false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        patrolTimePassed = patrolWait;
        lastPlaceSeen = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        State tmpstate = curState; // temporary variable to check if the state has changed

        // -- Field of View logic --
        if (ICanSee(player))
        {
            curState = State.Chase;
            lastPlaceSeen = player.position;
        }
        else
        {
            if (curState == State.Chase)
            {
                curState = State.Investigate;
            }
        }

        // -- State check --
        switch (curState)
        {
            case State.Patrol: // Start patrolling
                Patrol();
                break;
            case State.Investigate:
                Investigate();
                break;
            case State.Chase: // Move towards the player
                Chase(player);
                break;
        }
        // If the state has changed, log it
        if (tmpstate != curState)
        {
            Debug.Log("Guard's state: " + curState);
        }
    }

    void Chase(Transform player)
    {
        GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
        GetComponent<UnityEngine.AI.NavMeshAgent>().ResetPath();

        Vector3 direction = player.position - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(direction), Time.deltaTime * chasingRotSpeed);

        if (direction.magnitude > chasingAccuracy)
        {
            transform.Translate(0, 0, Time.deltaTime * chasingSpeed);
        }
    }

    void Investigate()
    {
        // If the agent arrived at the investigating goal,
        // they should start patrolling there
        if (transform.position == lastPlaceSeen)
        {
            curState = State.Patrol;
        }
        else
        {
            GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(lastPlaceSeen);
            Debug.Log("Guard's state: " + curState + " point " + lastPlaceSeen);
        }
    }

    void Patrol()
    {
        patrolTimePassed += Time.deltaTime;

        if (patrolTimePassed > patrolWait)
        {
            patrolTimePassed = 0; // reset the timer
            Vector3 patrollingPoint = lastPlaceSeen;

            // Generate a random point on the X,Z axis at 'patrolDistance' distance from the lastPlaceSeen position
            patrollingPoint += new Vector3(Random.Range(-patrolDistance, patrolDistance), 0, Random.Range(-patrolDistance, patrolDistance));

            // Make the generated point a goal for the agent
            GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(patrollingPoint);
        }
    }
    
    public void InvestigatePoint(Vector3 point)
    { 
        lastPlaceSeen = point;
        curState = State.Investigate;
    }

    // Draw the field of view cone in the Scene view (always visible)
    void OnDrawGizmos()
    {
        if (!showFOVGizmos)
            return;
        // Set color based on whether the guard can see the player
        if (player != null && ICanSee(player))
        {
            Gizmos.color = Color.red; // Red when player is detected
        }
        else
        {
            Gizmos.color = Color.yellow; // Yellow when no player detected
        }

        // Draw the cone
        Vector3 forward = transform.forward * fovDistance;
        Vector3 startPos = transform.position;

        // Calculate the left and right boundaries of the cone
        Quaternion leftRayRotation = Quaternion.AngleAxis(-fovAngle, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(fovAngle, Vector3.up);

        Vector3 leftRayDirection = leftRayRotation * forward;
        Vector3 rightRayDirection = rightRayRotation * forward;

        // Draw the cone lines
        Gizmos.DrawLine(startPos, startPos + leftRayDirection);
        Gizmos.DrawLine(startPos, startPos + rightRayDirection);
        Gizmos.DrawLine(startPos, startPos + forward);

        // Draw an arc to represent the cone's range
        int segments = 20;
        Vector3 previousPoint = startPos + leftRayDirection;

        for (int i = 1; i <= segments; i++)
        {
            float angle = -fovAngle + (2 * fovAngle * i / segments);
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 direction = rotation * forward;
            Vector3 point = startPos + direction;

            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        // Optional: Draw a ray to the player if visible
        if (player != null && ICanSee(player))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(startPos, player.position);
        }
    }
}   
