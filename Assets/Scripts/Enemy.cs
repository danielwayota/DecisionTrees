/**

ya hemos visto un poco IA moderna con las redes neuronales

y hoy toca ver un "sistema" de hacer IA en juegos algo más tradicional
y que se usa extensamente en la industra hoy en día.

Los árboles de decisión.

El concepto es sencillo y lo bueno es que pueden llegar a ser muy complejos PERO predecibles.

Veamos este ejemplo:

Que como hemos visto en los vídeos anteriores, es sencillo enseñarles a hacer tareas simples,
como reaccionar al entorno
pero cuando queremos hacer algo más complejo, como el vídeo anterior, la cosa se complica bastante.

Así que una

 */



using UnityEngine;
using UnityEngine.AI;

public enum EnemyStatus {
    IDLE, ATTACK
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class Enemy: MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;

    [Header("Perception")]
    public float fov = 1f;
    public int eyeCount = 3;
    public float viewDistance = 8f;

    private Vector3[] eyes;

    private Rigidbody body;
    private NavMeshAgent agent;

    [Header("Reaction time")]
    public float timeOut = 0.25f;

    private float time = 0;

    // "Memory"

    private EnemyStatus status;

    private Vector3 lastPlayerPosition;
    public float maxSearchingTime = 5;
    private float searchingTime = 0;

    // =======================================
    void Start()
    {
        this.body = GetComponent<Rigidbody>();
        this.agent = GetComponent<NavMeshAgent>();

        this.eyes = new Vector3[this.eyeCount];

        this.status = EnemyStatus.IDLE;
    }

    // =======================================
    void Update()
    {
        this.time += Time.deltaTime;

        if (this.searchingTime > 0)
        {
            this.searchingTime -= Time.deltaTime;
        }

        if (this.time >= this.timeOut)
        {
            this.time = 0;

            this.CollectEyesSensorData();

            switch (this.status)
            {
                case EnemyStatus.IDLE:
                    float angle = Random.Range(0f, 360f);

                    this.transform.rotation = Quaternion.Euler(0, angle, 0);

                    float distance = Random.Range(1f, 3f);
                    Vector3 targetPosition = this.transform.position + this.transform.forward * distance;


                    this.agent.SetDestination(targetPosition);
                break;

                case EnemyStatus.ATTACK:
                    this.agent.SetDestination(this.lastPlayerPosition);

                    float distanceToTarget = Vector3.Distance(this.lastPlayerPosition, this.transform.position);

                    if (this.searchingTime <= 0)
                    {
                        this.status = EnemyStatus.IDLE;
                    }
                break;
            }
        }
    }

    // =======================================
    void CollectEyesSensorData()
    {
        // Calculate the eyes ray direction
        Vector3 front = this.transform.forward;
        float baseAngle = Mathf.Atan2(front.z, front.x);

        int index = 0;
        // Calculate the base angle and the offset for the three eyes

        int sideEyeCount = Mathf.FloorToInt(this.eyeCount / 2f);

        float fovPercent = this.fov / sideEyeCount;

        for (int i = -sideEyeCount; i <= sideEyeCount; i++)
        {
            float angle = baseAngle + (fovPercent * i);
            Vector3 lookDir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

            this.eyes[index] = lookDir;
            index++;
        }

        // Detect what each eye 'sees' and put the data in the sensor
        // If nothing is seen, just put 0

        Vector3 position = this.transform.position;

        foreach (var lookDir in this.eyes)
        {
            RaycastHit hit;

            // Three front eyes

            if (Physics.Raycast(position + (lookDir * 0.5f), lookDir, out hit, this.viewDistance))
            {
                GameObject go = hit.collider.gameObject;

                Player sh = go.GetComponent<Player>();

                if (sh != null)
                {
                    Debug.DrawLine(position, position + (lookDir * this.viewDistance), Color.green, 0.5f);

                    this.status = EnemyStatus.ATTACK;

                    this.lastPlayerPosition = sh.transform.position;
                    this.searchingTime = this.maxSearchingTime;
                }
                else
                {
                    Debug.DrawLine(position, position + (lookDir * this.viewDistance), Color.white, 0.5f);
                }
            }
            else
            {
                Debug.DrawLine(position, position + (lookDir * this.viewDistance), Color.black, 0.5f);
            }
        }
    }
}
