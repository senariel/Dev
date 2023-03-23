using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public GameObject scrollView;

    // 플레이어가 보유 중인 카드 목록
    public List<GameObject> myDeck;
    // 카드 분배 딜레이
    public float dealDelay = 2.0f;
    // 시작 시 분배받는 카드 갯수
    public int initialDealCards = 5;

    protected UnityEngine.UI.ScrollRect rect;
    protected GameManager gameManager = null;
    protected List<GameObject> playerDeck = new();


    void Awake()
    {
        GameObject gm = GameObject.Find("GameManager");
        if (!gm)
            return;

        gameManager = gm.GetComponent<GameManager>();
        if (gameManager)
        {
            gameManager.Inventory = this;
        }

        rect = scrollView.GetComponent<UnityEngine.UI.ScrollRect>();

        // 카드 준비
        MakePlayerDeck();
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < initialDealCards; ++i)
        {
            DealCard();
        }

        StartCoroutine(StartDealCard());
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 플레이어에게 부여할 카드 덱을 생성하기
    void MakePlayerDeck()
    {
        playerDeck.Clear();

        // 카드를 인스턴스로 생성해서 배열에 랜덤하게 집어 넣기
        for (int i = 0; i < myDeck.Count; ++i)
        {
            playerDeck.Add(Instantiate(myDeck[i]));
        }

        Debug.Log("[Inventory : MakePlayerDeck] " + playerDeck.Count + " / " + myDeck.Count);
    }

    // 카드 분배 루프
    protected IEnumerator StartDealCard()
    {
        while(playerDeck.Count > 0)
        {
            // 선 딜레이
            yield return new WaitForSeconds(dealDelay);

            DealCard();
        }

        yield break;
    }

    protected void DealCard()
    {
        if (playerDeck.Count <= 0)
            return;

        int index = Random.Range(0, playerDeck.Count);
        GameObject obj = playerDeck[index];
        playerDeck.RemoveAt(index);

        TakeCard(obj?.GetComponent<Card>());
    }

    // 카드 받기
    public void TakeCard(Card card)
    {
        if (!card)
            return;

        card.gameObject.transform.SetParent(rect.content.transform);
    }
}
