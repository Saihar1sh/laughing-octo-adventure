using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CardState
{
    Hidden,
    Revealing,
    Revealed,
    Matched
}

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class CardController : MonoBehaviour, IPointerClickHandler
{
    [Header("Visuals")]
    [SerializeField] private Image faceImage;
    [SerializeField] private Image backImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField] private float flipDuration = 0.25f;
    [SerializeField] private float matchedFadeDuration = 0.4f;
    [SerializeField] private AnimationCurve flipCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public int PairId { get; private set; }
    public CardState State { get; private set; } = CardState.Hidden;
    public bool IsBusy => State == CardState.Revealing || State == CardState.Matched;

    private RectTransform rectTransform;
    private CancellationTokenSource flipCts;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Initialize(int pairId, Sprite face)
    {
        PairId = pairId;
        faceImage.sprite = face;
        
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        SetHiddenInstant();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (State != CardState.Hidden)
            return;

        DeckManager.Instance.NotifyCardClicked(this);
    }

    public async Task FlipOpen()
    {
        if (State != CardState.Hidden)
            return;

        CancelFlip();
        flipCts = new CancellationTokenSource();

        State = CardState.Revealing;
        await FlipAsync(true, flipCts.Token);
    }

    public async void FlipClose()
    {
        if (State != CardState.Revealed)
            return;

        CancelFlip();
        flipCts = new CancellationTokenSource();

        State = CardState.Revealing;
        await FlipAsync(false, flipCts.Token);
    }

    private async Task FlipAsync(bool open, CancellationToken token)
    {
        float half = flipDuration * 0.5f;
        float t = 0f;

        while (t < half)
        {
            if (token.IsCancellationRequested) return;
            t += Time.deltaTime;
            float v = 1f - flipCurve.Evaluate(t / half);
            rectTransform.localScale = new Vector3(v, 1f, 1f);
            await Task.Yield();
        }

        ShowFaceCard(open);

        t = 0f;
        while (t < half)
        {
            if (token.IsCancellationRequested) return;
            t += Time.deltaTime;
            float v = flipCurve.Evaluate(t / half);
            rectTransform.localScale = new Vector3(v, 1f, 1f);
            await Task.Yield();
        }

        rectTransform.localScale = Vector3.one;
        State = open ? CardState.Revealed : CardState.Hidden;
    }

    public async void SetMatched()
    {
        CancelFlip();
        State = CardState.Matched;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float t = 0f;
        while (t < matchedFadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1f - (t / matchedFadeDuration);
            await Task.Yield();
        }
        canvasGroup.alpha = 0f;
    }

    private void SetHiddenInstant()
    {
        CancelFlip();
        State = CardState.Hidden;
        rectTransform.localScale = Vector3.one;
        ShowFaceCard(false);
        canvasGroup.interactable = true;
    }

    private void ShowFaceCard(bool show = true)
    {
        faceImage.gameObject.SetActive(show);
        backImage.gameObject.SetActive(!show);
    }

    private void CancelFlip()
    {
        if (flipCts == null) return;
        flipCts.Cancel();
        flipCts.Dispose();
        flipCts = null;
    }

    private void OnDestroy()
    {
        CancelFlip();
    }
}