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

    [Header("Prefabs")]
    [SerializeField] private CardController cardPrefab;
    [SerializeField] private RectTransform spacerPrefab; // empty invisible spacer

    [Header("Layout Target")]
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private GridLayoutGroup gridLayout;

    [Header("Grid Dimensions")]
    [SerializeField] private int rows = 4;
    [SerializeField] private int cols = 4;

    [Header("Base Layout (Design Space)")]
    [SerializeField] private Vector2 baseCardSize = new Vector2(160, 230);
    [SerializeField] private Vector2 baseSpacing = new Vector2(12, 12);
    [SerializeField] private Vector2 basePadding = new Vector2(20, 20);

    [Header("Card Faces")]
    [SerializeField] private List<Sprite> faceSprites;

    private readonly List<CardController> allCards = new();
    private readonly Queue<CardController> revealQueue = new();

    private const float MismatchCloseDelay = 0.6f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartNewLayout(5, 5, 453);
    }
    
    public void StartNewLayout(int r, int c, int? seed = null)
    {
        rows = r;
        cols = c;

        ClearBoard();
        GenerateBoard(seed);
    }
    
    private void ClearBoard()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        allCards.Clear();
        revealQueue.Clear();
    }

    private void GenerateBoard(int? seed)
    {
        gridLayout.enabled = true;
        ConfigureResponsiveLayout();

        int totalSlots = rows * cols;
        bool needsSpacer = totalSlots % 2 != 0;

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
        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        pairIds = pairIds.OrderBy(_ => rng.Next()).ToList();

        int spacerIndex = needsSpacer ? GetCenterIndex(rows, cols) : -1;
        int pairCursor = 0;

        for (int slot = 0; slot < totalSlots; slot++)
        {
            if (slot == spacerIndex)
            {
                Instantiate(spacerPrefab, gridParent);
                continue;
            }

            int pairId = pairIds[pairCursor++];
            CardController card = Instantiate(cardPrefab, gridParent);

            Sprite face = faceSprites.Count > 0
                ? faceSprites[pairId % faceSprites.Count]
                : null;

            card.Initialize(pairId, face);
            allCards.Add(card);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(gridParent);
        gridLayout.enabled = false;
    }
    
    private void ConfigureResponsiveLayout()
    {
        Rect rect = gridParent.rect;

        float desiredWidth =
            cols * baseCardSize.x +
            (cols - 1) * baseSpacing.x +
            basePadding.x * 2;

        float desiredHeight =
            rows * baseCardSize.y +
            (rows - 1) * baseSpacing.y +
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
        gridLayout.constraintCount = cols;
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

        await card.FlipOpen();

        if (card.State != CardState.Revealed)
            return;

        revealQueue.Enqueue(card);
        ResolveRevealQueue();
    }

    private void ResolveRevealQueue()
    {
        if (revealQueue.Count < 2)
            return;

        CardController a = revealQueue.Dequeue();
        CardController b = revealQueue.Dequeue();

        if (a == b)
            return;

        if (a.PairId == b.PairId)
        {
            a.SetMatched();
            b.SetMatched();

            if (allCards.All(c => c.State == CardState.Matched))
            {
                // TODO: Game Over
                Debug.Log("Game Over");
            }
        }
        else
        {
            CloseAfterDelay(a, b);
        }
    }

    private async void CloseAfterDelay(CardController a, CardController b)
    {
        await Task.Delay(TimeSpan.FromSeconds(MismatchCloseDelay));

        if (a.State == CardState.Revealed)
            a.FlipClose();
        if (b.State == CardState.Revealed)
            b.FlipClose();
    }
}
