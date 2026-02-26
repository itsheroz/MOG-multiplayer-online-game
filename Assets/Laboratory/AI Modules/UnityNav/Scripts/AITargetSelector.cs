using UnityEngine;
using System.Collections;

public class AITargetSelector : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private float scanInterval = 0.2f; // สแกนหาศัตรูทุกๆ 0.2 วินาที (5 ครั้งต่อวิ)
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private string playerTag = "Player";

    [Header("Debug")]
    [SerializeField] private Transform currentTarget; // เก็บค่าล่าสุดที่หาเจอ
    private Transform forcedTarget;

    private void Start()
    {
        // เริ่มต้นการสแกนอัตโนมัติทันทีที่เกิด
        StartCoroutine(ScanRoutine());
    }

    // Coroutine ทำงานแยกอิสระจาก Brain
    private IEnumerator ScanRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(scanInterval);

        while (true)
        {
            // ถ้ามี Forced Target (เช่น จากระบบ Aggro) ไม่ต้องสแกนหาเอง
            if (forcedTarget == null)
            {
                FindClosestPlayer();
            }
            else
            {
                // ตรวจสอบว่า Forced Target ยังอยู่ดีไหม
                if (!forcedTarget.gameObject.activeInHierarchy)
                {
                    forcedTarget = null; // เป้าหมายตาย/หายไป กลับไปสแกนใหม่
                }
                else
                {
                    currentTarget = forcedTarget;
                }
            }

            yield return wait; // รอจนกว่าจะครบเวลาแล้วทำใหม่
        }
    }

    // Brain เรียกฟังก์ชันนี้: จะได้ค่าทันที ไม่กิน Performance เพิ่ม
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    public void SetForcedTarget(Transform target)
    {
        forcedTarget = target;
        currentTarget = forcedTarget; // อัปเดตทันทีเพื่อให้ AI ตอบสนองไว
    }

    public void ClearTarget()
    {
        forcedTarget = null;
        currentTarget = null; // รีเซ็ต current ด้วย เพื่อให้เริ่มสแกนใหม่ในรอบหน้า
    }

    private void FindClosestPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        Transform bestTarget = null;
        float closestDistSqr = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(playerTag))
            {
                Vector3 direction = hit.transform.position - currentPos;
                float dSqrToTarget = direction.sqrMagnitude;

                if (dSqrToTarget < closestDistSqr)
                {
                    closestDistSqr = dSqrToTarget;
                    bestTarget = hit.transform;
                }
            }
        }

        // อัปเดตตัวแปรกลาง
        currentTarget = bestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
            Gizmos.DrawSphere(currentTarget.position + Vector3.up * 2, 0.5f);
        }
    }
}