using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }


    [Header("Prefabs & Parents")]
    [SerializeField] private CardController cardPrefab;
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private GridLayoutGroup gridLayout;


    [Header("Layout")]
    public int rows = 4;
    public int cols = 4;
    public Vector2 spacing = new Vector2(8,8);
    public Vector2 padding = new Vector2(10,10);


    [Header("Sprites")]
    public List<Sprite> faceSprites; 


    [Header("Runtime")]
    public List<CardController> allCards = new List<CardController>();


    Queue<CardController> revealQueue = new Queue<CardController>();


    float mismatchCloseDelay = 0.6f;


    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
        StartNewLayout(5,5,453);
    }


    public void StartNewLayout(int r, int c, int? seed = null)
    {
        rows = r; 
        cols = c;
        
        ClearBoard();
        GenerateBoard(seed);
    }


    void ClearBoard()
    {
        foreach (var c in allCards) { if (c) Destroy(c.gameObject); }
        allCards.Clear();
        revealQueue.Clear();
    }


    void GenerateBoard(int? seed)
    {

        int cells = rows * cols;
        if (cells % 2 != 0) cells--; // ensure even cards


        List<int> pairIds = new List<int>();
        int pairs = cells / 2;
        for (int i = 0; i < pairs; i++) { pairIds.Add(i); pairIds.Add(i); }


// shuffle
        System.Random rnd = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        pairIds = pairIds.OrderBy(x => rnd.Next()).ToList();


// compute card size
        Vector2 parentSize = (gridParent.rect.size);
        float availableWidth = parentSize.x - padding.x * 2 - spacing.x * (cols - 1);
        float availableHeight = parentSize.y - padding.y * 2 - spacing.y * (rows - 1);
        float cardW = availableWidth / cols;
        float cardH = availableHeight / rows;
        float cardSize = Mathf.Min(cardW, cardH);


        int index = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (index >= pairIds.Count) break;
                int pairId = pairIds[index];
                var go = Instantiate(cardPrefab, gridParent);
                go.name = $"Card_{r}_{c}_{pairId}";
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(cardSize, cardSize);


                Sprite face = faceSprites.Count > 0 ? faceSprites[pairId % faceSprites.Count] : null;
                go.Initialize(pairId, face);
                allCards.Add(go);
                index++;
            }
        }
    }


    public async void NotifyCardClicked(CardController card)
    {
        if (card == null) return;
        await card.FlipOpen();
        revealQueue.Enqueue(card);
        TryResolveQueue();
       //TODO: Audio
    }
    
    void TryResolveQueue()
    {
// resolve pairs in FIFO order
        while (revealQueue.Count >= 2)
        {
            var a = revealQueue.Dequeue();
            var b = revealQueue.Dequeue();
// If either is already matched, skip and continue
            if (a.State == CardState.Matched || b.State == CardState.Matched) continue;


            //Matched
            if (a.PairId == b.PairId)
            {
                a.SetMatched();
                b.SetMatched();
                
                //TODO: Score on matched



                if (allCards.Count(x => x != null && x.gameObject.activeSelf && x.State == CardState.Matched) >= allCards.Count)
                {
                    //TODO: Game over condition
                }
            }
            //Mismatched
            else
            {
                CloseAfterDelay(a, b);
                //TODO: Mismatch handle
            }
        }
    }


    async void CloseAfterDelay(CardController a, CardController b)
    {
        await Task.Delay(TimeSpan.FromSeconds(mismatchCloseDelay));
        
        if (a.State == CardState.Revealed) 
             a.FlipClose();
        if (b.State == CardState.Revealed) 
            b.FlipClose();
    }



}