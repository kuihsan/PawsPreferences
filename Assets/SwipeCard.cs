using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class SwipeCard : MonoBehaviour
{
    public float swipeThreshold = 150f;
    public float moveSpeed = 10f;

    private Vector2 startPos;
    private bool isDragging;
    private RectTransform rect;

    public System.Action<bool> OnSwipe; // true = like, false = dislike
    public CanvasGroup likeCG;
    public CanvasGroup dislikeCG;

    public float indicatorThreshold = 50f;
    public float maxSwipeDistance = 300f;

    public AudioSource audioSource;
    public AudioClip likeClip;
    public AudioClip dislikeClip;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Input.touchSupported && Input.touchCount > 0)
            TouchInput();
        else
            MouseInput();
    }


    void TouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began)
        {
            startPos = t.position;
            isDragging = true;
        }
        else if (t.phase == TouchPhase.Moved && isDragging)
        {
            Vector2 delta = t.position - startPos;
            rect.anchoredPosition = delta;
            rect.rotation = Quaternion.Euler(0, 0, delta.x * -0.05f);

            UpdateIndicators(delta.x);
        }

        else if (t.phase == TouchPhase.Ended)
        {
            EndSwipe();
        }
    }

    void MouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - startPos;
            rect.anchoredPosition = delta;
            rect.rotation = Quaternion.Euler(0, 0, delta.x * -0.05f);

            UpdateIndicators(delta.x);
        }

        else if (Input.GetMouseButtonUp(0))
        {
            EndSwipe();
        }
    }
    void UpdateIndicators(float x)
    {
        float normalized = Mathf.Clamp(x / maxSwipeDistance, -1f, 1f);

        if (normalized > 0)
        {
            // LIKE
            likeCG.alpha = Mathf.Abs(normalized);
            likeCG.transform.localScale = Vector3.one * (0.8f + 0.2f * normalized);

            dislikeCG.alpha = 0;
        }
        else
        {
            // DISLIKE
            dislikeCG.alpha = Mathf.Abs(normalized);
            dislikeCG.transform.localScale = Vector3.one * (0.8f + 0.2f * Mathf.Abs(normalized));

            likeCG.alpha = 0;
        }
    }
    void ResetIndicators()
    {
        likeCG.DOFade(0f, 0.15f);
        dislikeCG.DOFade(0f, 0.15f);

        likeCG.transform.DOScale(0.8f, 0.15f);
        dislikeCG.transform.DOScale(0.8f, 0.15f);
    }

    void EndSwipe()
    {
        isDragging = false;

        if (Mathf.Abs(rect.anchoredPosition.x) > swipeThreshold)
        {
            bool liked = rect.anchoredPosition.x > 0;

            // Play audio
            if (liked && likeClip != null)
                audioSource.PlayOneShot(likeClip);
            else if (!liked && dislikeClip != null)
                audioSource.PlayOneShot(dislikeClip);

            // Optional punch animation
            if (liked)
                likeCG.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
            else
                dislikeCG.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);

            OnSwipe?.Invoke(liked);
        }
        else
        {
            rect.anchoredPosition = Vector2.zero;
            rect.rotation = Quaternion.identity;
        }

        ResetIndicators();
    }


}
