using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : NetworkBehaviour
{
    public float minWanderRadius = 1f;
    public float maxWanderRadius = 4f;
    public float minPauseDuration = 1.5f;
    public float maxPauseDuration = 6f;
    public float pauseDuration;
    
    public Animator anim;
    private NavMeshAgent agent;
    private float timer;
    private bool isPausing;
    
    private float smoothSpeed;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        pauseDuration = Random.Range(minPauseDuration, maxPauseDuration);
        timer = pauseDuration;
    }

    void Update() {
        if (!IsServer) return;
        
        if (isPausing) {
            timer -= Time.deltaTime;
            if (timer <= 0) {
                SetNewRandomDestination();
                isPausing = false;
            }
        } 
        else {
            // Check if we have arrived
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) {
                isPausing = true;
                timer = pauseDuration;
            }
        }
        
        // Calculate the current speed (0 to MaxSpeed)
        // agent.velocity.magnitude gives the actual movement speed
        float currentSpeed = agent.velocity.magnitude;

        // Optional: Map the speed to a 0-1 range if your Blend Tree uses normalized values
        float normalizedSpeed = currentSpeed / agent.speed;

        // Update the Animator parameter
        anim.SetFloat("Speed", normalizedSpeed);
    }

    void SetNewRandomDestination() {
        float wanderRadius = Random.Range(minWanderRadius, maxWanderRadius);
        Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
        randomDir += transform.position;
        
        NavMeshHit hit;
        // Find the nearest valid point on the NavMesh
        if (NavMesh.SamplePosition(randomDir, out hit, wanderRadius, 1)) {
            agent.SetDestination(hit.position);
        }
    }
}
