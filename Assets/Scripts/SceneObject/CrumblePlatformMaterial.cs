using System.Collections;
using UnityEngine;

public class CrumblePlatformMaterial : MonoBehaviour
{
    [Header("基礎設定")]
    [SerializeField] private float fallDelay = 1.0f;     // 踩到後多久消失
    [SerializeField] private float respawnTime = 3.0f;   // 消失後多久重生

    [Header("震動效果設定")]
    [SerializeField] private float shakeMagnitude = 0.1f; // 震動幅度
    [SerializeField] private float shakeSpeed = 50.0f;    // 震動頻率

    private Vector3 originalPosition;
    private MeshRenderer meshRenderer;
    private Collider platformCollider;
    private bool isCrumbling = false;

    void Start()
    {
        originalPosition = transform.position;
        meshRenderer = GetComponent<MeshRenderer>();
        platformCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 重要：檢查玩家的 Tag 是否為 "Player"
        if (collision.gameObject.CompareTag("Player") && !isCrumbling)
        {
            StartCoroutine(CrumbleRoutine());
        }
    }

    private IEnumerator CrumbleRoutine()
    {
        isCrumbling = true;
        float elapsed = 0f;

        while (elapsed < fallDelay)
        {
            // 震動時變紅
            meshRenderer.material.color = Color.red;
            // 使用正弦波產生規律的快速擺動
            float shake = Mathf.Sin(Time.time * shakeSpeed) * shakeMagnitude;

            // 讓地板在 X 和 Z 軸同時快速擺動
            transform.position = originalPosition + new Vector3(shake, 0, shake);

            elapsed += Time.deltaTime;
            yield return null;
        }
        // --- 震動效果結束 ---

        // 隱藏地板
        meshRenderer.enabled = false;
        platformCollider.enabled = false;

        // 等待重生
        yield return new WaitForSeconds(respawnTime);

        // 恢復地板
        transform.position = originalPosition;
        meshRenderer.enabled = true;
        platformCollider.enabled = true;
        isCrumbling = false;
    }
}