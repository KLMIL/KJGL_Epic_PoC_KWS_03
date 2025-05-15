/**********************************************************
 * Script Name: Interactable
 * Author: 김우성
 * Date Created: 2025-05-15
 * Last Modified: 2025-05-15
 * Description: 
 * - 모든 도구의 공통 상호작용 정의
 *********************************************************/

using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] float _interactionRange = 2f;
    [SerializeField] LayerMask _playerLayer;

    protected Inventory Inventory { get; private set; }
    protected InventoryUI InventoryUI { get; private set; }

    protected virtual void Start()
    {
        Inventory = FindFirstObjectByType<Inventory>();
        InventoryUI = FindFirstObjectByType<InventoryUI>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics2D.OverlapCircle(transform.position, _interactionRange, _playerLayer))
            {
                Interact();
            }
        }
    }

    protected abstract void Interact();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _interactionRange);
    }
}
