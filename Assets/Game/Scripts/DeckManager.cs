using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Prefabs & Parents")] [SerializeField]
    private CardController cardPrefab;

    [SerializeField] private RectTransform gridParent;
    [SerializeField] private GridLayoutGroup gridLayout;

    [Header("Layout")] [SerializeField] private int rows = 4;
    [SerializeField] private int cols = 4;
    [SerializeField] private Vector2 spacing = new Vector2(8, 8);
    [SerializeField] private Vector2 padding = new Vector2(10, 10);

    [Header("Sprites")] [SerializeField] private List<Sprite> faceSprites;

    private readonly List<CardController> allCards = new();
    private readonly Queue<CardController> revealQueue = new();
    private const float CardAspect = 0.7f; // width / height
    
    [SerializeField] private Vector2 baseCardSize = new Vector2(160, 230);
    [SerializeField] private Vector2 baseSpacing = new Vector2(12, 12);
    [SerializeField] private Vector2 basePadding = new Vector2(20, 20);


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
        StartNewLayout(rows, cols, 453);
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
        foreach (var card in allCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        allCards.Clear();
        revealQueue.Clear();
    }

    private void GenerateBoard(int? seed)
    {
        ConfigureGridLayout();
        int totalCells = rows * cols;
        if (totalCells % 2 != 0)
            totalCells--; // ensure even

        // Pair IDs
        List<int> pairIds = new();
        for (int i = 0; i < totalCells / 2; i++)
        {
            pairIds.Add(i);
            pairIds.Add(i);
        }

        // Shuffle deterministically if seed exists
        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        pairIds = pairIds.OrderBy(_ => rng.Next()).ToList();

        // Spawn cards
        for (int i = 0; i < pairIds.Count; i++)
        {
            int pairId = pairIds[i];
            CardController card = Instantiate(cardPrefab, gridParent);
            card.name = $"Card_{i}_{pairId}";

            Sprite face = faceSprites.Count > 0
                ? faceSprites[pairId % faceSprites.Count]
                : null;

            card.Initialize(pairId, face);
            allCards.Add(card);
        }

        // Force layout once, then freeze it
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridParent);
        gridLayout.enabled = false;
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
    private void ConfigureGridLayout()
    {
        Rect rect = gridParent.rect;

        float layoutWidth =
            cols * baseCardSize.x +
            (cols - 1) * baseSpacing.x +
            basePadding.x * 2;

        float layoutHeight =
            rows * baseCardSize.y +
            (rows - 1) * baseSpacing.y +
            basePadding.y * 2;

        float scaleX = rect.width / layoutWidth;
        float scaleY = rect.height / layoutHeight;

        float scale = Mathf.Min(scaleX, scaleY);

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


    private async void CloseAfterDelay(CardController a, CardController b)
    {
        await Task.Delay(TimeSpan.FromSeconds(MismatchCloseDelay));

        if (a.State == CardState.Revealed)
            a.FlipClose();
        if (b.State == CardState.Revealed)
            b.FlipClose();
    }
}