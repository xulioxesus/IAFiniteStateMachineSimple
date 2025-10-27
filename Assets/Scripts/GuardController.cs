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

    // Chasing settings

    public float chasingSpeed = 2.0f;
    public float chasingRotSpeed = 2.0f;
    public float chasingAccuracy = 5.0f;

    // Patrol settings
    public float patrolDistance = 10.0f;
    float patrolWait = 5.0f;
    float patrolTimePassed = 0;

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
        GetComponent<UnityEngine.AI.NavMeshAgent>().Stop();
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
}   
