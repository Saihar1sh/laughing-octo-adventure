using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum CardState
{
    Hidden,
    Revealing,
    Revealed,
    Matched
}
[RequireComponent(typeof(RectTransform))]
public class CardController : MonoBehaviour, IPointerClickHandler
{
    [Header("Visuals")]
    [SerializeField] private Image faceImage;
    [SerializeField] private Image backImage;

    [Header("Flip Animation")]
    [SerializeField] private float flipDuration = 0.25f;
    [SerializeField] private AnimationCurve flipCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    public int PairId { get; private set; }
    public CardState State { get; private set; } = CardState.Hidden;

    RectTransform rectTransform;
    CancellationTokenSource flipCts;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(int pairId, Sprite faceSprite)
    {
        PairId = pairId;
        faceImage.sprite = faceSprite;
        SetHiddenInstant();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (State != CardState.Hidden) return;

        DeckManager.Instance.NotifyCardClicked(this);
    }

    public async Task FlipOpen()
    {
        if (State != CardState.Hidden) return;

        CancelFlip();
        flipCts = new CancellationTokenSource();

        State = CardState.Revealing;
        await FlipAsync(open: true, flipCts.Token);
    }

    public async void FlipClose()
    {
        if (State != CardState.Revealed) return;

        CancelFlip();
        flipCts = new CancellationTokenSource();

        State = CardState.Revealing;
        await FlipAsync(open: false, flipCts.Token);
    }

    async Task FlipAsync(bool open, CancellationToken token)
    {
        float half = flipDuration * 0.5f;
        float elapsed = 0f;

        // First half: scale X → 0
        while (elapsed < half)
        {
            if (token.IsCancellationRequested) return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            float scaleX = 1f - flipCurve.Evaluate(t);
            rectTransform.localScale = new Vector3(scaleX, 1f, 1f);

            await Task.Yield();
        }

        // Swap visuals at midpoint
        backImage.gameObject.SetActive(!open);
        faceImage.gameObject.SetActive(open);

        // Second half: scale X → 1
        elapsed = 0f;
        while (elapsed < half)
        {
            if (token.IsCancellationRequested) return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            float scaleX = flipCurve.Evaluate(t);
            rectTransform.localScale = new Vector3(scaleX, 1f, 1f);

            await Task.Yield();
        }

        rectTransform.localScale = Vector3.one;
        State = open ? CardState.Revealed : CardState.Hidden;
    }

    public void SetMatched()
    {
        CancelFlip();
        State = CardState.Matched;

        faceImage.gameObject.SetActive(true);
        backImage.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    void SetHiddenInstant()
    {
        CancelFlip();

        State = CardState.Hidden;
        rectTransform.localScale = Vector3.one;

        faceImage.gameObject.SetActive(false);
        backImage.gameObject.SetActive(true);
    }

    void CancelFlip()
    {
        if (flipCts != null)
        {
            flipCts.Cancel();
            flipCts.Dispose();
            flipCts = null;
        }
    }

    void OnDestroy()
    {
        CancelFlip();
    }
}