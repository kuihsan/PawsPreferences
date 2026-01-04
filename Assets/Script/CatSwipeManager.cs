using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;


public class CatSwipeManager : MonoBehaviour
{
    [Header("Cat Image")]
    public RawImage currentCardImage;
    public RawImage nextCardImage;
    public SwipeCard swipeCard;
    public GameObject endPanel;
    public TMP_Text summaryTMP;
    public Transform likedContainer;
    public GameObject likedImagePrefab;
    [Header("Loading UI")]
    public GameObject loadingPanel;
    public Scrollbar loadingScrollbar;
    public TMP_Text loadingText;

    public int totalCats = 10;
    public int preloadCount = 3;

    private int shownCount = 0;
    private Queue<Texture2D> catQueue = new Queue<Texture2D>();
    private List<Texture2D> likedCats = new List<Texture2D>();
    public CatService catService;

    void Start()
    {
        endPanel.SetActive(false);
        swipeCard.OnSwipe += OnSwiped;
        swipeCard.enabled = false;
        StartCoroutine(PreloadCats(10));

    }

    IEnumerator PreloadCats(int count)
    {
        loadingPanel.SetActive(true);
        swipeCard.enabled = false;
        DOTween.Kill(loadingScrollbar);
        loadingScrollbar.size = 0f;
        catQueue.Clear();

        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(catService.LoadCat(tex =>
            {
                if (tex != null)
                    catQueue.Enqueue(tex);
            }));


            float progress = (i + 1) / (float)count;
            DOTween.To(
                () => loadingScrollbar.size,
                x => loadingScrollbar.size = x,
                progress,
                0.2f
            ).SetEase(Ease.OutQuad);

            if (loadingText != null)
                loadingText.text = $"Loading cats... {(int)(progress * 100)}%";
        }

        // Assign first two cards
        currentCardImage.texture = catQueue.Dequeue();
        nextCardImage.texture = catQueue.Dequeue();
        shownCount = 1;

        loadingPanel.SetActive(false);
        loadingText.text = "Loading cats...0%";
        swipeCard.enabled = true;
    }



    IEnumerator LoadAndEnqueue()
    {
        yield return StartCoroutine(catService.LoadCat(tex =>
        {
            catQueue.Enqueue(tex);
        }));
    }

    void OnSwiped(bool liked)
    {
        if (liked)
        {
            likedCats.Add((Texture2D)currentCardImage.texture);
        }

        AdvanceCards();
    }

    void AdvanceCards()
    {
        if (shownCount >= totalCats)
        {
            ShowSummary();
            return;
        }

        // Move next → current
        currentCardImage.texture = nextCardImage.texture;
        ResetCardTransform();

        // Queue → next
        if (catQueue.Count > 0)
        {
            nextCardImage.texture = catQueue.Dequeue();
        }

        shownCount++;

        // Keep queue full
        StartCoroutine(LoadAndEnqueue());
    }

    void ResetCardTransform()
    {
        currentCardImage.rectTransform.anchoredPosition = Vector2.zero;
        currentCardImage.rectTransform.rotation = Quaternion.identity;
    }

    void ShowSummary()
    {
        swipeCard.enabled = false;

        // Reset panel state
        endPanel.SetActive(true);

        CanvasGroup cg = endPanel.GetComponent<CanvasGroup>();
        cg.alpha = 0;
        endPanel.transform.localScale = Vector3.one * 0.8f;

        // Animate
        cg.DOFade(1f, 0.35f);
        endPanel.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);

        summaryTMP.text = $"You liked {likedCats.Count} out of {totalCats} cats!";

        foreach (var tex in likedCats)
        {
            GameObject img = Instantiate(likedImagePrefab, likedContainer);
            img.GetComponent<RawImage>().texture = tex;
        }
    }

    public void RestartGame()
    {
        endPanel.SetActive(false);

        shownCount = 0;
        likedCats.Clear();
        catQueue.Clear();

        foreach (Transform child in likedContainer)
            Destroy(child.gameObject);

        currentCardImage.texture = null;
        nextCardImage.texture = null;

        ResetCardTransform();
        StartCoroutine(PreloadCats(totalCats));
    }




}
