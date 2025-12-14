// =========================
// DeckManager.cs (FINAL REWRITE)
// =========================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Prefabs")] [SerializeField] private CardController cardPrefab;
    [SerializeField] private RectTransform spacerPrefab; // empty invisible spacer

    [Header("Layout Target")] [SerializeField]
    private RectTransform gridParent;

    [SerializeField] private GridLayoutGroup gridLayout;


    [Header("Base Layout (Design Space)")] [SerializeField]
    private Vector2 baseCardSize = new Vector2(160, 230);

    [SerializeField] private Vector2 baseSpacing = new Vector2(12, 12);
    [SerializeField] private Vector2 basePadding = new Vector2(20, 20);

    [Header("Card Faces")] [SerializeField]
    private List<Sprite> faceSprites;

    private int _rows = 4;
    private int _cols = 4;

    private readonly List<CardController> _activeCards = new();
    private readonly Queue<CardController> _revealQueue = new();
    private readonly Stack<CardController> _cardPool = new();

    private const float MISMATCH_CLOSE_DELAY = 0.6f;

    private int _currentSeed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public async void StartNewLayout(int r, int c, int? seed = null, List<CardSaveData> saveData = null)
    {
        _rows = r;
        _cols = c;

        var seedValue = seed ?? UnityEngine.Random.Range(0, int.MaxValue);
        _currentSeed = seedValue;

        ClearBoard();
        await GenerateBoard(saveData);
    }

    private void ClearBoard()
    {
        foreach (Transform child in gridParent)
        {
            var card = child.GetComponent<CardController>();
            if (card != null)
            {
                ReturnCardToPool(card);
            }
            else
            {
                Destroy(child.gameObject); // spacer
            }
        }

        _activeCards.Clear();
        _revealQueue.Clear();
    }

    private async Task GenerateBoard(List<CardSaveData> cardsSaveData = null)
    {
        gridLayout.enabled = true;
        ConfigureResponsiveLayout();

        int totalSlots = _rows * _cols;
        bool needsSpacer = totalSlots % 2 != 0;
        bool hasSavedData = false;
        if (cardsSaveData != null)
        {
            totalSlots = cardsSaveData.Count;
            hasSavedData = true;
        }

        int playableCards = needsSpacer ? totalSlots - 1 : totalSlots;
        int pairCount = playableCards / 2;

        // Build pair IDs
        List<int> pairIds = new();
        for (int i = 0; i < pairCount; i++)
        {
            pairIds.Add(i);
            pairIds.Add(i);
        }

        // Shuffle
        System.Random rng = new System.Random(_currentSeed);
        pairIds = pairIds.OrderBy(_ => rng.Next()).ToList();

        int spacerIndex = needsSpacer ? GetCenterIndex(_rows, _cols) : -1;
        int pairCursor = 0;
        CardSaveData cardSaveData = null;
        for (int slot = 0; slot < totalSlots; slot++)
        {
            if (slot == spacerIndex)
            {
                Instantiate(spacerPrefab, gridParent);
                continue;
            }

            if (hasSavedData)
                cardSaveData = cardsSaveData[slot];

            CardController card = GetCardFromPool();
            card.transform.SetParent(gridParent, false);


            int pairId = !hasSavedData ? pairIds[pairCursor++] : cardSaveData.pairId;
            Sprite face = faceSprites.Count > 0 ? faceSprites[pairId % faceSprites.Count] : null;


            card.Initialize(pairId, face);
            card.name = "Card " + pairId;
            _activeCards.Add(card);
            if (hasSavedData)
                card.SetState(Enum.Parse<CardState>(cardSaveData.state));
        }

        await Task.Yield();

        LayoutRebuilder.ForceRebuildLayoutImmediate(gridParent);

        gridLayout.enabled = false;
    }

    private void ConfigureResponsiveLayout()
    {
        Rect rect = gridParent.rect;

        float desiredWidth =
            _cols * baseCardSize.x +
            (_cols - 1) * baseSpacing.x +
            basePadding.x * 2;

        float desiredHeight =
            _rows * baseCardSize.y +
            (_rows - 1) * baseSpacing.y +
            basePadding.y * 2;

        float scale = Mathf.Min(
            rect.width / desiredWidth,
            rect.height / desiredHeight
        );

        Vector2 finalCardSize = baseCardSize * scale;
        Vector2 finalSpacing = baseSpacing * scale;
        Vector2 finalPadding = basePadding * scale;

        gridLayout.cellSize = finalCardSize;
        gridLayout.spacing = finalSpacing;
        gridLayout.padding = new RectOffset(
            Mathf.RoundToInt(finalPadding.x),
            Mathf.RoundToInt(finalPadding.x),
            Mathf.RoundToInt(finalPadding.y),
            Mathf.RoundToInt(finalPadding.y)
        );

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = _cols;
    }

    private int GetCenterIndex(int r, int c)
    {
        int centerRow = r / 2;
        int centerCol = c / 2;
        return centerRow * c + centerCol;
    }

    public async void NotifyCardClicked(CardController card)
    {
        if (card == null || card.IsBusy)
            return;

        AudioManager.Instance.PlayFlip();
        await card.FlipOpen();

        if (card.State != CardState.Revealed)
            return;

        _revealQueue.Enqueue(card);
        ResolveRevealQueue();
    }

    private void ResolveRevealQueue()
    {
        if (_revealQueue.Count < 2)
            return;

        CardController a = _revealQueue.Dequeue();
        CardController b = _revealQueue.Dequeue();

        if (a == b)
            return;

        if (a.PairId == b.PairId)
        {
            a.SetMatched();
            b.SetMatched();
            ScoreManager.Instance.OnMatch();
            AudioManager.Instance.PlayMatch();

            if (_activeCards.All(c => c.State == CardState.Matched))
            {
                // TODO: Game Over
                GameManager.Instance.OnGameOver();

                Debug.Log("Game Over");
            }
        }
        else
        {
            ScoreManager.Instance.OnMismatch();
            AudioManager.Instance.PlayMismatch();

            CloseAfterDelay(a, b);
        }
    }

    private async void CloseAfterDelay(CardController a, CardController b)
    {
        await Task.Delay(TimeSpan.FromSeconds(MISMATCH_CLOSE_DELAY));

        if (a.State == CardState.Revealed)
            a.FlipClose();
        if (b.State == CardState.Revealed)
            b.FlipClose();
    }

    public DeckSaveData CaptureDeckState()
    {
        var data = new DeckSaveData
        {
            rows = _rows,
            cols = _cols,
            seed = _currentSeed,
            cards = new List<CardSaveData>()
        };
        foreach (var c in _activeCards)
        {
            if (c == null) continue;
            data.cards.Add(new CardSaveData
            {
                pairId = c.PairId,
                state = c.State.ToString(),
                active = c.gameObject.activeSelf
            });
        }

        return data;
    }
    
    private CardController GetCardFromPool()
    {
        if (_cardPool.Count > 0)
        {
            var card = _cardPool.Pop();
            card.gameObject.SetActive(true);
            return card;
        }


        return Instantiate(cardPrefab);
    }


    private void ReturnCardToPool(CardController card)
    {
        card.ResetState();
        card.gameObject.SetActive(false);
        card.transform.SetParent(transform, false);
        _cardPool.Push(card);
    }
}

[System.Serializable]
public class CardSaveData
{
    public int pairId;
    public string state;
    public bool active;
}

[System.Serializable]
public class DeckSaveData
{
    public int rows;
    public int cols;
    public int seed;
    public List<CardSaveData> cards;
}