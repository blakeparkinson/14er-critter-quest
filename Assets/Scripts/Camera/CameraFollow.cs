using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, -10f);
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSpeed = 3f;

    [Header("Bounds")]
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 200f;
    [SerializeField] private float minX = -20f;
    [SerializeField] private float maxX = 50f;

    private float currentLookAhead;

    private void LateUpdate()
    {
        if (target == null) return;

        float inputX = Input.GetAxisRaw("Horizontal");
        float targetLookAhead = inputX * lookAheadDistance;
        currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);

        Vector3 desiredPos = target.position + offset;
        desiredPos.x += currentLookAhead;
        desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
        desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);

        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }
}
