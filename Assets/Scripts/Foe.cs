using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum FoeStatus {
    SEARCH, ATTACK
}

public class Foe : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;

    private NavMeshAgent agent;

    private FoeStatus status;

    [Header("Reaction time")]
    public float timeOut = 0.25f;

    private float time = 0;

    [Header("Perception")]
    public float fov = 1f;
    public int eyeCount = 3;
    public float viewDistance = 8f;

    private Vector3[] eyes;

    private Vector3 lastPlayerPosition;
    public float maxSearchingTime = 5;
    private float searchingTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        this.agent = GetComponent<NavMeshAgent>();

        this.status = FoeStatus.SEARCH;

        this.eyes = new Vector3[this.eyeCount];
    }

    // Update is called once per frame
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
                case FoeStatus.SEARCH:
                    float angle = Random.Range(0f, 360f);

                    this.transform.rotation = Quaternion.Euler(0, angle, 0);

                    float distance = Random.Range(1f, 3f);
                    Vector3 targetPosition = this.transform.position + this.transform.forward * distance;


                    this.agent.SetDestination(targetPosition);
                break;
                case FoeStatus.ATTACK:
                    this.agent.SetDestination(this.lastPlayerPosition);

                    float distanceToTarget = Vector3.Distance(this.lastPlayerPosition, this.transform.position);

                    if (distanceToTarget < 0.1f)
                    {
                        if (this.searchingTime <= 0)
                        {
                            this.status = FoeStatus.SEARCH;
                        }
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

                    this.status = FoeStatus.ATTACK;

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
