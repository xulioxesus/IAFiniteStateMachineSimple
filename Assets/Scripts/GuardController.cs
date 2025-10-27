using UnityEngine;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))] // Require un NavMeshAgent para o movemento
public class GuardController : MonoBehaviour // Controlador de gardián con FSM (Finite State Machine)
{
    enum State { Patrol, Investigate, Chase }; // Estados da FSM: Patrullar, Investigar, Perseguir
    State curState = State.Patrol; // Estado actual, comeza en Patrol

    Vector3 lastPlaceSeen; // Última posición onde se viu o xogador
    public Transform player; // Referencia ao transform do xogador
    float fovDistance = 20.0f; // Distancia do campo de visión
    float fovAngle = 45.0f; // Ángulo do campo de visión (en graos)

    [Header("Debug/Visualization")]
    public bool showFOVGizmos = true; // Mostrar o campo de visión en pantalla

    public float chasingSpeed = 2.0f; // Velocidade ao perseguir
    public float chasingRotSpeed = 2.0f; // Velocidade de rotación ao perseguir
    public float chasingAccuracy = 5.0f; // Distancia mínima ao xogador ao perseguir

    public float patrolDistance = 10.0f; // Radio de patrulla
    public float patrolWait = 5.0f; // Tempo de espera entre puntos de patrulla
    float patrolTimePassed = 0; // Tempo pasado desde o último cambio de punto

    public float knockRadius = 20.0f; // Radio para detectar sons/ruídos

    bool ICanSee(Transform player) // Comproba se o gardián pode ver o xogador
    {
        Vector3 direction = player.position - this.transform.position; // Dirección ao xogador
        float angle = Vector3.Angle(direction, this.transform.forward); // Ángulo entre fronte e xogador

        RaycastHit hit;
        if (
            Physics.Raycast(this.transform.position, direction, out hit) && // Lanza un raio cara o xogador
            hit.collider.gameObject.tag == "Player" && // O raio acertou o xogador?
            direction.magnitude < fovDistance && // Está o xogador dentro da distancia do FOV?
            angle < fovAngle // Está o xogador dentro do ángulo do FOV?
        )
        {
            return true;
        }
        return false;
    }

    void Start()
    {
        patrolTimePassed = patrolWait; // Inicia listo para escoller un punto de patrulla
        lastPlaceSeen = transform.position; // A última posición vista é a actual
    }

    void Update()
    {
        State tmpstate = curState; // Garda o estado actual para detectar cambios

        if (ICanSee(player)) // Se ve o xogador
        {
            curState = State.Chase; // Cambia a perseguir
            lastPlaceSeen = player.position; // Actualiza a última posición vista
        }
        else
        {
            if (curState == State.Chase) // Se estaba perseguindo e xa non ve o xogador
            {
                curState = State.Investigate; // Cambia a investigar
            }
        }

        switch (curState) // Executa a lóxica do estado actual
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Investigate:
                Investigate();
                break;
            case State.Chase:
                Chase(player);
                break;
        }
        
        if (tmpstate != curState) // Se o estado cambiou
        {
            Debug.Log("Guard's state: " + curState); // Rexistra o novo estado
        }

        DrawFOVDebug(); // Debuxa o FOV en play mode
    }

    void Chase(Transform player) // Persegue o xogador
    {
        GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true; // Para o NavMeshAgent
        GetComponent<UnityEngine.AI.NavMeshAgent>().ResetPath(); // Limpa o camiño

        Vector3 direction = player.position - transform.position; // Dirección ao xogador
        transform.rotation = Quaternion.Slerp(transform.rotation, // Rota suavemente cara o xogador
            Quaternion.LookRotation(direction), Time.deltaTime * chasingRotSpeed);

        if (direction.magnitude > chasingAccuracy) // Se está lonxe dabondo
        {
            transform.Translate(0, 0, Time.deltaTime * chasingSpeed); // Move cara adiante
        }
    }

    void Investigate() // Investiga a última posición onde se viu o xogador
    {
        float distanceToTarget = Vector3.Distance(transform.position, lastPlaceSeen); // Distancia ao obxectivo
        
        if (distanceToTarget < GetComponent<UnityEngine.AI.NavMeshAgent>().stoppingDistance + 0.5f) // Se chegou ao punto
        {
            curState = State.Patrol; // Cambia a patrullar
        }
        else
        {
            GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(lastPlaceSeen); // Move cara o punto a investigar
            Debug.Log("Guard's state: " + curState + " point " + lastPlaceSeen);
        }
    }

    void Patrol() // Patrulla por puntos aleatorios
    {
        patrolTimePassed += Time.deltaTime; // Incrementa o contador de tempo

        if (patrolTimePassed > patrolWait) // Se pasou o tempo de espera
        {
            patrolTimePassed = 0; // Reinicia o contador
            Vector3 patrollingPoint = lastPlaceSeen; // Punto de patrulla baseado na última posición

            patrollingPoint += new Vector3(Random.Range(-patrolDistance, patrolDistance), 0, Random.Range(-patrolDistance, patrolDistance)); // Xera un punto aleatorio

            GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(patrollingPoint); // Establece o destino
        }
    }

    public void InvestigatePoint(Vector3 point) // Ordena ao gardián investigar un punto específico
    {
        lastPlaceSeen = point; // Establece o punto a investigar
        curState = State.Investigate; // Cambia a estado Investigate
    }
    
    void DrawFOVDebug() // Debuxa o FOV en play mode usando Debug.DrawLine
    {
        if (!showFOVGizmos)
            return;

        Color lineColor = (player != null && ICanSee(player)) ? Color.red : Color.yellow; // Vermello se ve o xogador, amarelo se non

        Vector3 forward = transform.forward * fovDistance; // Dirección frontal do FOV
        Vector3 startPos = transform.position; // Posición inicial

        Quaternion leftRayRotation = Quaternion.AngleAxis(-fovAngle, Vector3.up); // Rotación para o bordo esquerdo
        Quaternion rightRayRotation = Quaternion.AngleAxis(fovAngle, Vector3.up); // Rotación para o bordo dereito

        Vector3 leftRayDirection = leftRayRotation * forward; // Dirección do bordo esquerdo
        Vector3 rightRayDirection = rightRayRotation * forward; // Dirección do bordo dereito

        Debug.DrawLine(startPos, startPos + leftRayDirection, lineColor); // Debuxa o bordo esquerdo
        Debug.DrawLine(startPos, startPos + rightRayDirection, lineColor); // Debuxa o bordo dereito
        Debug.DrawLine(startPos, startPos + forward, lineColor); // Debuxa a liña central

        int segments = 20; // Número de segmentos do arco
        Vector3 previousPoint = startPos + leftRayDirection;

        for (int i = 1; i <= segments; i++) // Debuxa o arco do FOV
        {
            float angle = -fovAngle + (2 * fovAngle * i / segments); // Calcula o ángulo de cada segmento
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 direction = rotation * forward;
            Vector3 point = startPos + direction;

            Debug.DrawLine(previousPoint, point, lineColor); // Debuxa o segmento
            previousPoint = point;
        }

        if (player != null && ICanSee(player)) // Se ve o xogador
        {
            Debug.DrawLine(startPos, player.position, Color.red); // Debuxa unha liña ata o xogador
        }
    }

    void OnDrawGizmos() // Debuxa o FOV na vista Scene do editor
    {
        if (!showFOVGizmos)
            return;
        
        if (player != null && ICanSee(player)) // Se ve o xogador
        {
            Gizmos.color = Color.red; // Vermello cando detecta o xogador
        }
        else
        {
            Gizmos.color = Color.yellow; // Amarelo cando non detecta o xogador
        }

        Vector3 forward = transform.forward * fovDistance; // Dirección frontal do FOV
        Vector3 startPos = transform.position; // Posición inicial

        Quaternion leftRayRotation = Quaternion.AngleAxis(-fovAngle, Vector3.up); // Rotación para o bordo esquerdo
        Quaternion rightRayRotation = Quaternion.AngleAxis(fovAngle, Vector3.up); // Rotación para o bordo dereito

        Vector3 leftRayDirection = leftRayRotation * forward; // Dirección do bordo esquerdo
        Vector3 rightRayDirection = rightRayRotation * forward; // Dirección do bordo dereito

        Gizmos.DrawLine(startPos, startPos + leftRayDirection); // Debuxa o bordo esquerdo
        Gizmos.DrawLine(startPos, startPos + rightRayDirection); // Debuxa o bordo dereito
        Gizmos.DrawLine(startPos, startPos + forward); // Debuxa a liña central

        int segments = 20; // Número de segmentos do arco
        Vector3 previousPoint = startPos + leftRayDirection;

        for (int i = 1; i <= segments; i++) // Debuxa o arco do FOV
        {
            float angle = -fovAngle + (2 * fovAngle * i / segments); // Calcula o ángulo de cada segmento
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 direction = rotation * forward;
            Vector3 point = startPos + direction;

            Gizmos.DrawLine(previousPoint, point); // Debuxa o segmento
            previousPoint = point;
        }

        if (player != null && ICanSee(player)) // Se ve o xogador
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(startPos, player.position); // Debuxa unha liña ata o xogador
        }
    }
}