using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public float speed = 2f;

    void Update()
    {
        // Move object right continuously
        transform.position += Vector3.right * speed * Time.deltaTime;
    }
}
