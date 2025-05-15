/**********************************************************
 * Script Name: Crafter
 * Author: 김우성
 * Date Created: 2025-05-15
 * Last Modified: 2025-05-15
 * Description: 
 * - 서로 다른 아이템 2개를 합쳐 상위 레벨 아이템을 만듦
 *********************************************************/

using UnityEngine;

public class Crafter : Interactable
{
    [SerializeField] GameObject _crafterPanelPrefab;
    GameObject _crafterPanelInstance;

    protected override void Start()
    {
        base.Start();
        _crafterPanelInstance = Instantiate(_crafterPanelPrefab, InventoryUI.transform);
        _crafterPanelInstance.SetActive(false);
    }

    protected override void Interact()
    {
        InventoryUI.ShowCrafterPanel(_crafterPanelInstance);
        Debug.Log("Opend Crafter");
    }
}
