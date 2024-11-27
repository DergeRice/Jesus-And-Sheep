using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System;

public class GameLogicManager : MonoBehaviour
{
    public static GameLogicManager instance;
    public bool isPlayerTurn = true;
    public BlockManager blockManager;
    public GameObject ballPrefab; // 공 프리팹
    public GameObject debugBall; // 디버그 공 프리팹
    public Transform spawnPoint; // 공 생성 위치
    public LineRenderer trajectoryLine; // 궤적을 표시하는 라인 렌더러

    private Vector3 dragStart; // 드래그 시작 위치
    private Vector3 dragEnd; // 드래그 끝 위치
    private bool isDragging = false;

    public int maxBounceCount = 2;      // 최대 튕김 횟수
    public LayerMask collisionLayer;    // 충돌이 발생할 레이어

    public List<Ball> ballList = new List<Ball>();

    public int ballCount;

    public GameObject jesus;
    private float nextXvalue;

    public int currentLevel = 1;


    public Vector2 currentPoint;       // CircleCast 시작 위치
    public Vector2 currentDirection;  // CircleCast 방향
    public float radius = 0.3f;       // CircleCast 반지름

    public GameCanvas gameCanvas;

    private Vector3 clampedDragDirection;

    public TMP_Text ballCountText, currentLevelText;

    public Action turnEndAction;

    public EventManager eventManager;

    private void Awake()
    {
        instance = this;
        debugBall.transform.position = Vector3.one * 300f;
        gameCanvas.getBallDownAction = GetAllBallDown;
        gameCanvas.getBallDownButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isPlayerTurn == true) HandleInput();
        ballCountText.text = ballCount.ToString();
        currentLevelText.text = currentLevel.ToString();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0)) // 드래그 시작
        {
            dragStart = GetMouseWorldPosition();
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging) // 드래그 중
        {
            float width = trajectoryLine.startWidth;
            trajectoryLine.material.mainTextureScale = new Vector2(1f / width, 1.0f);

            Vector3 currentDragPosition = GetMouseWorldPosition();
            Vector3 dragDirection = currentDragPosition - spawnPoint.transform.position;

            // 드래그 방향의 각도 계산 (SignedAngle로 계산, 기준 벡터는 오른쪽)
            float angle = Mathf.Atan2(dragDirection.y, dragDirection.x) * Mathf.Rad2Deg;

            // 각도를 0 ~ 360도로 정규화
            angle = (angle + 360) % 360;

            // 각도를 170도 ~ 10도 범위로 제한
            if (angle > 170f || angle < 10f)
            {
                if (angle > 180f)
                {
                    angle = 170f; // 상한 제한
                }
                else
                {
                    angle = 10f;  // 하한 제한
                }
            }

            // 제한된 각도로 방향 재계산
            float angleInRadians = angle * Mathf.Deg2Rad;
            dragDirection = new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians), 0f);

            // 제한된 방향을 저장 (드래그 끝날 때 사용)
            clampedDragDirection = dragDirection;

            // 디버그 출력
            //Debug.Log($"Clamped Angle: {angle}");

            // 궤적 그리기
            DrawTrajectory(spawnPoint.transform.position, dragDirection * dragDirection.magnitude);
            trajectoryLine.enabled = true;
        }
        else if (Input.GetMouseButtonUp(0) && isDragging) // 드래그 끝
        {
            // 제한된 방향으로 발사
            Vector3 launchDirection = clampedDragDirection.normalized;

            // 공 생성 코루틴 시작
            gameCanvas.getBallDownButton.gameObject.SetActive(true);
            StartCoroutine(SpawnBallCount(launchDirection, ballCount));
            ClearTrajectory();

            isPlayerTurn = false;
            Debug.Log("MyTurn End");
            isDragging = false;
            debugBall.transform.position = Vector3.one * 300f;
        }

    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition; // 화면 좌표 (Screen space)
        mousePosition.z = 0f; // 2D에서는 Z축이 필요 없으므로 0으로 고정
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    private void SpawnBall(Vector3 direction)
    {
        // 공 생성
        direction = new Vector3(direction.x, direction.y, 0).normalized;
        GameObject ball = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);

        Ball ballScript = ball.GetComponent<Ball>();
        ballList.Add(ballScript);
        ballScript.ballDownAction = BallComeDown;

        if (ballScript != null)
        {
            ballScript.Initialize(direction); // 공의 방향 초기화
        }
    }

    IEnumerator SpawnBallCount(Vector3 direction, int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnBall(direction); // 공 생성
            yield return new WaitForSeconds(0.05f); // 일정 간격 대기
        }
    }

    private void DrawTrajectory(Vector3 start, Vector3 end)
    {
        List<Vector3> points = new List<Vector3>();
        currentPoint = start;
        currentDirection = end.normalized;

        points.Add(currentPoint); // 시작점 추가

        bool hitFirst = false; // 첫 번째 충돌을 찾았는지 체크

        for (int i = 0; i <= maxBounceCount; i++)
        {
            // CircleRay로 충돌 지점 계산
            RaycastHit2D hit = Physics2D.CircleCast(currentPoint, 0.15f, currentDirection, Mathf.Infinity, collisionLayer); // 0.3f radius로 원을 쏨

            if (hit.collider != null)
            {
                points.Add(hit.point); // 충돌 지점 추가

                // 첫 번째 충돌 지점에 디버그 공 배치
                if (!hitFirst)
                {
                    debugBall.transform.position = hit.point; // 디버그 공을 첫 충돌 지점에 배치
                    hitFirst = true; // 첫 충돌을 찾았음
                }

                // 충돌 후, 새로운 출발점을 약간 이동 (오프셋)
                currentPoint = hit.point + hit.normal * 0.01f; // 0.01f 오프셋 추가

                // 반사 벡터 계산 (법선에 대한 반사 계산)
                currentDirection = Vector2.Reflect(currentDirection, hit.normal);

                // 반사 벡터를 적용한 새로운 경로를 계산하여 추가
                points.Add(currentPoint + currentDirection * 3f);

                // 현재 방향이 반사 벡터로 올바르게 변경되었는지 확인 (디버깅용)
                Debug.DrawRay(currentPoint, currentDirection * 2, Color.green, 0.1f);
            }
            else
            {
                // 충돌이 없으면 3f 정도까지 그리기
                points.Add(currentPoint + currentDirection * 3f);
                break;
            }
        }

        // LineRenderer에 경로 설정 (3번째 점부터는 제외)
        trajectoryLine.positionCount = Mathf.Min(points.Count, 3); // 최대 3개의 점만 사용
        trajectoryLine.SetPositions(points.Take(3).ToArray()); // 첫 3개 점만 선택하여 설정
    }

    private void ClearTrajectory()
    {
        // 궤적 라인 렌더러 초기화
        trajectoryLine.positionCount = 0;
        trajectoryLine.enabled = false;
    }

    public void BallComeDown(Ball ball,float nextXvalue)
    {
        isPlayerTurn = false;
        ballList.Remove(ball);
        Utils.DelayCall(()=> Destroy(ball.gameObject), 0.7f);
        ball.rb.linearVelocity = Vector2.zero;
        //Destroy(ball.gameObject);
        this.nextXvalue = nextXvalue;

        if (ballList.Count == 0)
        {
            AllBallComeDown();
        }
    }

    public void AllBallComeDown()
    {
        currentLevel++;
        blockManager.BlockGetDown(currentLevel);
        Debug.Log("MyTurn");
        

        nextXvalue = Mathf.Clamp(nextXvalue ,- 2.5f,2.5f);
        gameCanvas.getBallDownButton.gameObject.SetActive(false);
        jesus.transform.DOMove(new Vector3(nextXvalue, jesus.transform.position.y,0),0.7f).SetEase(Ease.Linear);

        Utils.DelayCall( ()=>
        { 
            isPlayerTurn = true;
            turnEndAction?.Invoke();
            turnEndAction = null;
        },0.7f);
    }

    
    public void GetAllBallDown()
    {
        isPlayerTurn = false;
        foreach (var ball in ballList)
        {
            ball.GetComponent<CircleCollider2D>().isTrigger = true;
            ball.rb.linearVelocity = Vector2.down * 10;
        }
    }

    public void GetRandomSpecialBall(int amount)
    {
        Debug.Log($"you Got{amount} special ball");
    }

    public void ShowSelectPanel(GameEvent a, GameEvent b)
    {
        gameCanvas.selectPanel.ShowPanel(a,b);
    }
}
