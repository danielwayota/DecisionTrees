using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player: MonoBehaviour
{
    public float speed = 2f;

    private Rigidbody body;

    void Start()
    {
        this.body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(h, 0, v);

        this.body.velocity = movement.normalized * this.speed;
    }
}
