/**********************************************************
 * Script Name: PlayerController
 * Author: 김우성
 * Date Created: 2025-05-14
 * Last Modified: 2025-05-14
 * Description: 
 * - 입력에 캐릭터의 움직임 기능.
 * - (추후 PlayerManager로 이동가능)캐릭터의 상태 정보 저장.
 *********************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Move Parameter")]
    Rigidbody2D _rb;
    [SerializeField] float _walkSpeed = 5f;
    [SerializeField] float _runSpeed = 8f;

    [Header("Camera Parameter")]
    [SerializeField] Camera _mainCamera;
    [SerializeField] float _minZoom = 5f;
    [SerializeField] float _maxZoom = 10f;
    [SerializeField] float _zoomSpeed = 3f;

    /* 입력 관련 정보 */
    Vector2 _moveInput;
    bool _isRunning;

    [Header("Item Interaction")]
    [SerializeField] float _interactionRange = 2f;
    [SerializeField] LayerMask _itemLayer;

    [Header("Inventory")]
    [SerializeField] Inventory _inventory;
    [SerializeField] InventoryUI _inventoryUI;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        //_mainCamera = Camera.main; Inspector 할당 방식으로 변경
        _mainCamera.orthographicSize = _maxZoom;

        // 씬 전환 시 플레이어와 카메라 유지
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(_mainCamera.gameObject);

        // 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        InputUpdate();
    }

    private void FixedUpdate()
    {
        InputFixedUpdate();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새 씬의 SpawnPoint 찾기
        /* (추후) SpawnPoint가 아니라, 값 전달하는 방식으로 변경 */
        GameObject spawnPoint = GameObject.FindWithTag("SpawnPoint");
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
        }
        else
        {
            Debug.LogWarning("SpawnPoint Not Found");
        }
    }

    #region Input Area
    private void InputUpdate()
    {
        // 입력 처리
        _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        _isRunning = Input.GetKey(KeyCode.LeftShift);

        Look(); // 회전
        Interact(); // 아이템 상호작용
        Zoom(); // 마우스 줌인 줌아웃
    }

    private void InputFixedUpdate()
    {
        Move(); // 이동
    }


    private void Look()
    {
        Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Interact()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
        {
            PickUpItem();
        }
        if (Input.GetMouseButtonDown(1))
        {
            PlaceItem();
        }
        
        //// 분해 기능 테스트용 코드
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    _inventory.DecomposeItem(0, 0);
        //    _inventory.PrintInventory();
        //}

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("I Key Down");
            _inventoryUI.ToggleInventory();
        }
    }

    private void PickUpItem()
    {
        Collider2D itemCollider = Physics2D.OverlapCircle(transform.position, _interactionRange, _itemLayer);
        if (itemCollider != null)
        {
            Trash trash = itemCollider.GetComponent<Trash>();
            if (trash != null && _inventory.AddItem(trash.Item))
            {
                /* 아이템 획득 로직 추가 */
                Debug.Log($"Picked Up: {itemCollider.name}");
                _inventory.PrintInventory();
                Destroy(itemCollider.gameObject);
            }
            else
            {
                Debug.LogWarning($"No Trash component found on {itemCollider.name}");
            }
        }
    }

    private void PlaceItem()
    {
        Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        if (Vector2.Distance(transform.position, mousePos) <= _interactionRange)
        {
            /* 아이템 설치 로직 추가 */
            // 임시: 우클릭으로 병합 및 인벤토리 상태 출력
            _inventory.MergeElements();
            _inventory.PrintInventory();
        }
        else
        {
            Debug.Log("Too far to place item");
        }
    }

    private void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            float newSize = _mainCamera.orthographicSize - scroll * _zoomSpeed;
            _mainCamera.orthographicSize = Mathf.Clamp(newSize, _minZoom, _maxZoom);
        }
    }

    private void Move()
    {
        float currentSpeed = _isRunning ? _runSpeed : _walkSpeed;
        _rb.linearVelocity = _moveInput * currentSpeed;
    }

    // 씬에서만 보이는 기즈모
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _interactionRange);
    }

    #endregion


}
