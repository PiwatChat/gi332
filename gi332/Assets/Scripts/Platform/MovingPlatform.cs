using UnityEngine;

public enum Direction
{
    Horizontal, 
    Vertical
}
public class MovingPlatform : MonoBehaviour
{
    
    [SerializeField] private Direction moveDirection = Direction.Horizontal;
    [SerializeField] private float moveDistance = 5f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 1f;

    private Vector3 startPos;
    private Vector3 endPos;
    private bool movingToEnd = true;
    private float waitTimer = 0f;
    private Vector3 lastPos;
    private Vector3 platformVelocity;

    private void Start()
    {
        startPos = transform.position;
        endPos = moveDirection == Direction.Horizontal
            ? startPos + Vector3.right * moveDistance
            : startPos + Vector3.up * moveDistance;
        
        lastPos = transform.position;
    }

    private void FixedUpdate()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 target = movingToEnd ? endPos : startPos;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            movingToEnd = !movingToEnd;
            waitTimer = waitTime;
        }
        
        platformVelocity = (transform.position - lastPos) / Time.fixedDeltaTime;
        lastPos = transform.position;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 endPoint = Application.isPlaying ? endPos : (
            moveDirection == Direction.Horizontal
                ? transform.position + Vector3.right * moveDistance
                : transform.position + Vector3.up * moveDistance
        );

        Gizmos.DrawSphere(transform.position, 0.2f); // start
        Gizmos.DrawSphere(endPoint, 0.2f); // end
        Gizmos.DrawLine(transform.position, endPoint);
    }
}
