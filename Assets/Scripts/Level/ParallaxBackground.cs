using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private float parallaxFactor = 0.5f;
    [SerializeField] private bool infiniteHorizontal = true;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    private float textureUnitSizeX;

    private void Start()
    {
        cameraTransform = UnityEngine.Camera.main.transform;
        lastCameraPosition = cameraTransform.position;

        if (infiniteHorizontal)
        {
            var sprite = GetComponent<SpriteRenderer>()?.sprite;
            if (sprite != null)
            {
                var texture = sprite.texture;
                textureUnitSizeX = texture.width / sprite.pixelsPerUnit * transform.localScale.x;
            }
        }
    }

    private void LateUpdate()
    {
        Vector3 delta = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(delta.x * parallaxFactor, delta.y * parallaxFactor, 0);
        lastCameraPosition = cameraTransform.position;

        if (infiniteHorizontal && textureUnitSizeX > 0)
        {
            float diff = cameraTransform.position.x - transform.position.x;
            if (Mathf.Abs(diff) >= textureUnitSizeX)
            {
                float offset = diff % textureUnitSizeX;
                transform.position = new Vector3(cameraTransform.position.x + offset, transform.position.y, transform.position.z);
            }
        }
    }
}
