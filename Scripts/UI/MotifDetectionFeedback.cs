using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MotifDetectionFeedback : MonoBehaviour
{
    [SerializeField] private Camera photoCamera;
    [SerializeField] private CameraMotifDetector motifDetector;

    [Header("Detection Ring")]
    [SerializeField] private Image indicatorImage;
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;

    [Header("Text Feedback")]
    [SerializeField] private TMP_Text feedbackText;


    private void OnEnable()
    {
        if (motifDetector != null)
        {
            motifDetector.DetectionResultChanged += HandleDetectionResultChanged;
        }
    }

    private void OnDisable()
    {
        if (motifDetector != null)
        {
            motifDetector.DetectionResultChanged -= HandleDetectionResultChanged;
        }
    }

    private void Update()
    {
        UpdateIndicatorImagePosition();
    }

    private void UpdateIndicatorImagePosition()
    {
        Vector3 screenPoint;
        Vector2 localPoint;

        switch (motifDetector.CurrentResult.Status)
        {
            case MotifStatus.Invalid:
                indicatorImage.transform.localPosition = Vector3.zero;
                break;
            default:
                screenPoint = photoCamera.WorldToScreenPoint(motifDetector.CurrentResult.MotifIdentity.transform.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(indicatorImage.transform.parent.GetComponent<RectTransform>(), screenPoint, null, out localPoint);
                indicatorImage.transform.localPosition = localPoint;
                break;
        }
    }

    private void HandleDetectionResultChanged(MotifDetectionResult result)
    {
        feedbackText.text = (result.Status == MotifStatus.Valid || result.Status == MotifStatus.Invalid) ? "" : result.Status.ToString().InsertSpaces();
        indicatorImage.color = (result.Status == MotifStatus.Valid) ? validColor : invalidColor;
    }
}