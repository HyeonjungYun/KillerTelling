using UnityEngine;

public class CardThrower : MonoBehaviour
{
    [Header("발사 설정")]
    public GameObject cardPrefab;     // 발사할 카드 프리팹 (Rigidbody 필수)
    public Transform launchPoint;     // 발사 시작 위치
    public float launchMultiplier = 1.5f; // 마우스 드래그 감도 조절

    [Header("궤적 설정")]
    public LineRenderer trajectoryLine; // 궤적을 그릴 LineRenderer
    [Range(10, 100)]
    public int trajectoryPoints = 50; // 궤적을 그릴 점의 개수 (정밀도)
    public float timeBetweenPoints = 0.1f; // 궤적 점 사이의 시간 간격
    public LayerMask obstacleLayers; // 궤적이 충돌을 감지할 레이어

    private Vector3 startMousePos;
    private Vector3 currentMousePos;
    private Vector3 launchVelocity;
    private bool isDragging = false;

    private CardPhysics cardPhysics; // 프리팹의 CardPhysics 스크립트를 참조
    private Rigidbody cardRigidbody; // 프리팹의 Rigidbody를 참조

    [Header("회전 설정")]
    public float rotationMultiplier = 100f; // 회전 속도 (이 값을 조절)

    [Header("물리 설정")]
    public float magnusStrength = 0.1f;

    void Start()
    {
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }

        // (↓ 이 부분을 추가하세요 ↓)
        // 프리팹에서 물리 정보를 미리 가져와서 궤적 계산에 사용합니다.
        if (cardPrefab != null)
        {
            cardRigidbody = cardPrefab.GetComponent<Rigidbody>();
        }

        if (cardRigidbody == null)
        {
            Debug.LogWarning("CardThrower: 'cardPrefab'에 'Rigidbody'가 없습니다. 궤적 계산이 부정확할 수 있습니다.");
        }
    }

    void Update()
    {
        // 1. 마우스를 처음 눌렀을 때
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            startMousePos = Input.mousePosition;

            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = true; // 궤적 보이기
            }
        }

        // 2. 마우스를 누르고 있는 동안 (드래그 중)
        if (isDragging && Input.GetMouseButton(0))
        {
            currentMousePos = Input.mousePosition;

            // 마우스 드래그 방향과 거리에 따라 발사 속도 계산
            // (이 부분은 게임의 카메라 뷰(2D, 3D, TopDown)에 따라 커스텀 필요)
            Vector3 dragVector = currentMousePos - startMousePos;

            // 예시: 화면 Y 드래그 -> 앞/위 (Z, Y), 화면 X 드래그 -> 좌/우 (X)
            // (슬링샷처럼 반대로 계산하려면 dragVector에 -1 곱하기)
            float launchPowerY = (dragVector.y / Screen.height) * launchMultiplier;
            float launchPowerX = (dragVector.x / Screen.width) * launchMultiplier;

            // Z축(앞)은 Y 드래그에 비례하게 설정 (예시)
            float launchPowerZ = launchPowerY * 1.5f;

            // 초기 속도 계산 (발사 지점의 로컬 방향 기준)
            launchVelocity = launchPoint.right * launchPowerX +    // 좌우 (Req 1)
                             launchPoint.up * launchPowerY +       // 상하
                             launchPoint.forward * launchPowerZ;   // 앞뒤

            // (LaunchCard 함수에서 사용한 Y축 회전과 동일하게)
            Vector3 expectedAngularVelocity = launchPoint.up * rotationMultiplier;

            // 궤적 그리기 (Req 2)
            if (trajectoryLine != null)
            {
                // DrawTrajectory에 이 회전 속도를 전달합니다.
                DrawTrajectory(launchPoint.position, launchVelocity, expectedAngularVelocity);
            }
        }

        // 3. 마우스에서 손을 떼었을 때
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            // 궤적 숨기기
            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = false;
            }

            // 카드 발사 (Req 3)
            LaunchCard();
        }
    }

    void LaunchCard()
    {
        GameObject cardInstance = Instantiate(cardPrefab, launchPoint.position, launchPoint.rotation);
        Rigidbody rb = cardInstance.GetComponent<Rigidbody>();

        if (rb != null)
        {
            CardPhysics cardPhysicsInstance = cardInstance.GetComponent<CardPhysics>();
            if (cardPhysicsInstance != null)
            {
                cardPhysicsInstance.magnusStrength = this.magnusStrength;
            }

            // 1. 발사
            rb.AddForce(launchVelocity, ForceMode.VelocityChange);

            // 2. 회전 
            rb.AddTorque(cardInstance.transform.up * rotationMultiplier, ForceMode.VelocityChange);
        }
        else
        {
            Debug.LogError("카드 프리팹에 Rigidbody가 없습니다!");
        }
    }

    // 궤적을 그리는 함수
    void DrawTrajectory(Vector3 startPos, Vector3 velocity, Vector3 angularVelocity)
    {
        if (trajectoryLine == null) return;

        // 경고: LayerMask가 설정되지 않았으면 충돌 감지를 못합니다.
        if (obstacleLayers == 0) // (obstacleLayers.value == 0)과 동일
        {
            Debug.LogWarning("CardThrower: 'Obstacle Layers'가 설정되지 않았습니다. 인스펙터에서 설정해주세요. 궤적이 벽을 감지할 수 없습니다.");
        }

        bool canPreviewMagnus = (cardRigidbody != null);

        trajectoryLine.positionCount = trajectoryPoints; // 1. 일단 최대 길이로 설정
        trajectoryLine.SetPosition(0, startPos);

        // 시뮬레이션 변수 초기화
        Vector3 currentPosition = startPos;
        Vector3 currentVelocity = velocity;
        float timeStep = Time.fixedDeltaTime;

        float cardMass = 1.0f;
        if (canPreviewMagnus)
        {
            cardMass = cardRigidbody.mass;
        }

        for (int i = 1; i < trajectoryPoints; i++)
        {
            // '현재 위치'를 '지난 위치'로 백업
            Vector3 lastPosition = currentPosition;

            // 1. 총 가속도 계산 (중력 + 마그누스)
            Vector3 totalAcceleration = Physics.gravity;
            if (canPreviewMagnus && currentVelocity.magnitude > 0 && angularVelocity.magnitude > 0)
            {
                float magnusStrength = this.magnusStrength;
                Vector3 magnusForce = Vector3.Cross(angularVelocity, currentVelocity) * magnusStrength;
                Vector3 magnusAcceleration = magnusForce / cardMass;
                totalAcceleration += magnusAcceleration;
            }

            // 2. 다음 스텝의 속도와 위치 계산
            currentVelocity += totalAcceleration * timeStep;
            currentPosition += currentVelocity * timeStep;

            // 3. (핵심!) 충돌 검사: '지난 위치'에서 '현재 위치'로 선(Linecast)을 쏴서 검사
            if (Physics.Linecast(lastPosition, currentPosition, out RaycastHit hit, obstacleLayers))
            {
                // 4. 충돌이 감지된 경우!
                trajectoryLine.SetPosition(i, hit.point); // 궤적의 마지막 점을 정확한 '충돌 지점'으로 설정
                trajectoryLine.positionCount = i + 1;     // LineRenderer의 점 개수를 (i+1)개로 "자르기"
                break;                                  // 루프를 중단하고 궤적 그리기를 멈춤
            }

            // 5. 충돌이 없는 경우
            trajectoryLine.SetPosition(i, currentPosition); // 궤적 점을 계속 그림
        }
    }
}