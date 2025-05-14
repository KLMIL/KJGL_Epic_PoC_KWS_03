/**********************************************************
 * Script Name: SceneChanger
 * Author: 김우성
 * Date Created: 2025-05-14
 * Last Modified: 2025-05-14
 * Description: 
 * - 플레이어가 트리거에 닿으면 지정된 씬으로 전환.
 * - 문, 포털 등 씬 전환 트리거에 사용.
 *********************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChnager : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] string _targetScene;
    [SerializeField] string _playerTag = "Player";

    private void Start()
    {
        if (_targetScene == null)
        {
            Debug.LogError("Target scene is not assigned");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(_playerTag))
        {
            /* (추후) Fade In, Fade Out 기능 추가 */
            /* (추후) Player Controller에 _spawnPointTag의 위치값 전달 */
            SceneManager.LoadScene(_targetScene);
        }
    }
}
