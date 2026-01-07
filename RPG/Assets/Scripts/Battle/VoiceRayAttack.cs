using UnityEngine;
using System.Collections;

public class VoiceRayAttack : MonoBehaviour
{
    [Header("Ray Visual Settings")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float rayLifetime = 0.5f;
    [SerializeField] private float rayWidth = 0.2f;
    [SerializeField] private AnimationCurve rayWidthCurve;

    [Header("Ray Colors")]
    [SerializeField] private Gradient ascendingGradient;
    [SerializeField] private Gradient descendingGradient;
    [SerializeField] private Gradient stableGradient;

    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.startWidth = rayWidth;
            lineRenderer.endWidth = rayWidth;
            lineRenderer.widthCurve = rayWidthCurve;
        }
    }

    public void FireRay(Vector3 startPos, Vector3 direction, VoiceController.PitchTrend trend, float distance = 10f)
    {
        if (lineRenderer == null) return;

        StartCoroutine(AnimateRay(startPos, startPos + direction * distance, trend));
    }

    private IEnumerator AnimateRay(Vector3 start, Vector3 end, VoiceController.PitchTrend trend)
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, start);

        switch (trend)
        {
            case VoiceController.PitchTrend.Ascending:
                lineRenderer.colorGradient = ascendingGradient;
                break;
            case VoiceController.PitchTrend.Descending:
                lineRenderer.colorGradient = descendingGradient;
                break;
            case VoiceController.PitchTrend.Stable:
                lineRenderer.colorGradient = stableGradient;
                break;
        }

        float elapsed = 0f;
        float animationDuration = 0.15f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            lineRenderer.SetPosition(1, Vector3.Lerp(start, end, t));
            yield return null;
        }

        lineRenderer.SetPosition(1, end);

        yield return new WaitForSeconds(rayLifetime - animationDuration);

        lineRenderer.enabled = false;
    }
}
