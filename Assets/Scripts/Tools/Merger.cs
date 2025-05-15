/**********************************************************
 * Script Name: Merger
 * Author: 김우성
 * Date Created: 2025-05-15
 * Last Modified: 2025-05-15
 * Description: 
 * - 동일한 아이템 2개를 합쳐 상위 레벨 아이템을 만듦
 *********************************************************/

using UnityEngine;

public class Merger : Interactable
{
    [SerializeField] GameObject _mergerPanelPrefab; // 병합기 UI 패널 프리펩
    GameObject _mergerPanelInstance;

    protected override void Start()
    {
        base.Start();
        /* InventoryUI를 알고있나? 있다면, 생성할 때 같은 Canvas에 생성하도록 */
        _mergerPanelInstance = Instantiate(_mergerPanelPrefab, InventoryUI.transform);
        _mergerPanelInstance.SetActive(false);
    }

    protected override void Interact()
    {
        InventoryUI.ShowMergerPanel(_mergerPanelInstance);
        Debug.Log("Opened Merger");
    }
}
