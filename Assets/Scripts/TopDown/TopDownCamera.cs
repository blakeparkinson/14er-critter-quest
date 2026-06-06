using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    private Transform target;
    private Camera cam;
    private float smoothSpeed = 4f;
    private float leadAhead = 1.5f;
    private float leadSmooth = 2f;
    private float baseSize = 7f;
    private float jogSize = 8.5f;

    private Vector3 currentLead;
    private float currentSize;

    public void SetTarget(Transform t)
    {
        target = t;
        if (target != null)
            transform.position = new Vector3(target.position.x, target.position.y + 1.5f, -10f);
    }

    private void Start()
    {
        cam = GetComponent<Camera>();
        currentSize = baseSize;
        if (cam != null) cam.orthographicSize = baseSize;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // lead ahead in movement direction
        var player = target.GetComponent<TopDownPlayer>();
        Vector2 vel = target.GetComponent<Rigidbody2D>()?.linearVelocity ?? Vector2.zero;
        Vector3 targetLead = Vector3.zero;
        if (vel.magnitude > 0.5f)
            targetLead = (Vector3)(vel.normalized * leadAhead);
        currentLead = Vector3.Lerp(currentLead, targetLead, leadSmooth * Time.deltaTime);

        // zoom out slightly when jogging
        float targetSize = (player != null && player.IsMoving && vel.magnitude > 4f) ? jogSize : baseSize;
        currentSize = Mathf.Lerp(currentSize, targetSize, 2f * Time.deltaTime);
        if (cam != null) cam.orthographicSize = currentSize;

        // smooth follow
        Vector3 desired = target.position + currentLead;
        desired.z = -10f;
        desired.y += 1f; // slight offset so player is in lower third

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);

        // background color shifts with altitude
        if (cam != null)
        {
            float progress = Mathf.Clamp01(target.position.y / 100f);
            Color bgColor;
            if (progress < 0.15f)
                bgColor = new Color(0.15f, 0.22f, 0.12f); // dark green
            else if (progress < 0.4f)
                bgColor = new Color(0.1f, 0.18f, 0.1f);   // deep forest
            else if (progress < 0.6f)
                bgColor = new Color(0.18f, 0.22f, 0.15f);  // alpine
            else if (progress < 0.8f)
                bgColor = new Color(0.22f, 0.24f, 0.25f);  // rocky grey
            else
                bgColor = new Color(0.28f, 0.32f, 0.38f);  // summit blue-grey

            cam.backgroundColor = Color.Lerp(cam.backgroundColor, bgColor, 2f * Time.deltaTime);
        }
    }
}
