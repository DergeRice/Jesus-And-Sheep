using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    public float speed = 10f; // 공의 이동 속도
    public Vector3 moveDirection; // 이동 방향 벡터
    public Action<Ball, float> ballDownAction;

    public Rigidbody2D rb; // Rigidbody2D 참조
    public bool speedMode = false;
    //public Action speedModeCommit;

    private int bounceTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Rigidbody2D를 초기 방향으로 설정
        rb.linearVelocity = moveDirection.normalized * speed;
    }

    public void Initialize(Vector3 direction)
    {
        moveDirection = direction.normalized;
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.transform.CompareTag("BottomLine"))
        {
            ballDownAction?.Invoke(this, transform.position.x); // 공이 아래쪽에 닿으면 동작 실행
            Destroy(rb);
            transform.rotation = Quaternion.Euler(0, 0, 0);
            return;
        }

        bounceTime++;


        Vector2 moveDirection = rb.linearVelocity.normalized; // linearVelocity가 아니라 velocity 사용
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;

        // 무한 튕김 방지: angle이 90도 근처(예: 85~95도)일 때 약간의 랜덤 값 추가
        if (Mathf.Abs(angle % 90) < 3f) // 각도가 90도와 가까운 경우
        {
            float randomOffset = -5f; // -5도에서 5도 사이의 랜덤 값
            angle += randomOffset;

            // 방향 벡터도 조정
            float radians = Mathf.Deg2Rad * (angle + 90f); // 다시 라디안으로 변환
            moveDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            rb.linearVelocity = moveDirection * rb.linearVelocity.magnitude; // 기존 속도 유지
            Debug.Log($"Current Direction: {moveDirection}, Angle: {angle}");
        }

        // Z축 회전 적용
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (bounceTime > 20 && speedMode == false)
        {
            rb.linearVelocity = moveDirection * speed * 2;
            speedMode = true;
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.transform.CompareTag("BottomLine"))
        {
            ballDownAction?.Invoke(this, transform.position.x); // 공이 아래쪽에 닿으면 동작 실행
            Destroy(rb);
            transform.rotation = Quaternion.Euler(0, 0, 0);
            return;
        }

    }
}
