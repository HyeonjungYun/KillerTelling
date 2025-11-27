using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CardPhysics : MonoBehaviour
{
    [HideInInspector] public float magnusStrength;

    private Rigidbody rb;
    private bool isStopped = false; // (추가!) 카드가 멈췄는지 확인하는 변수

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // (추가!) 카드가 멈췄다면(isStopped) 마그누스 효과를 계산하지 않습니다.
        if (isStopped)
        {
            return;
        }

        if (rb.linearVelocity.magnitude == 0 || rb.angularVelocity.magnitude == 0)
        {
            return;
        }

        // ... (기존 마그누스 효과 계산) ...
        Vector3 magnusForceDirection = Vector3.Cross(rb.angularVelocity, rb.linearVelocity);
        Vector3 magnusForce = magnusForceDirection * magnusStrength;
        rb.AddForce(magnusForce);
    }

    // (↓ 이 함수 전체가 새로 추가되었습니다 ↓)
    // 이 함수는 이 오브젝트의 콜라이더가 다른 콜라이더와 부딪혔을 때 
    // '최초 1회' 자동으로 호출됩니다.
    void OnCollisionEnter(Collision collision)
    {
        // 이미 멈춘 상태라면 아무것도 하지 않습니다.
        if (isStopped)
        {
            return;
        }

        // 부딪힌 오브젝트의 태그(Tag)가 "Wall"인지 확인합니다.
        if (collision.gameObject.CompareTag("Target"))
        {
            // 1. 충돌 정보(collision)에서 첫 번째 충돌 지점(contacts[0])을 가져옵니다.
            ContactPoint contactPoint = collision.contacts[0];

            // 2. 그 지점의 Vector3 좌표(point)를 가져옵니다.
            Vector3 hitPosition = contactPoint.point;

            // 3. 콘솔(Console) 창에 좌표를 출력합니다.
            // "F4"는 소수점 넷째 자리까지 깔끔하게 표시하는 서식입니다.
            Debug.Log("카드가 벽에 닿은 좌표: " + hitPosition.ToString("F4"));

            // "Wall" 태그가 맞다면, 카드를 즉시 멈춥니다.
            StopCard();
        }
    }

    // (↓ 이 함수도 새로 추가되었습니다 ↓)
    // 카드를 멈추는 역할을 하는 별도 함수
    void StopCard()
    {
        isStopped = true; // 멈춤 상태로 변경

        // 1. Rigidbody의 모든 움직임(속도)을 0으로 만듭니다.
        rb.linearVelocity = Vector3.zero;

        // 2. Rigidbody의 모든 회전을 0으로 만듭니다.
        rb.angularVelocity = Vector3.zero;

        // 3. (가장 중요) Rigidbody를 'Kinematic'으로 변경합니다.
        //    -> 'Kinematic'이 되면 더 이상 중력, 힘 등 어떤 물리 영향도 받지 않고
        //       그 자리에 "얼어붙게" 됩니다.
        rb.isKinematic = true;
    }
}