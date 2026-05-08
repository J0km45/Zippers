using UnityEngine;

public class PlayerClassDataCompareTest : MonoBehaviour
{
    [SerializeField] private PlayerStats _playerStats;
    [SerializeField] private int _classId = 10001;

    private void Start()
    {
        PlayerClassDataSO statsData = _playerStats.PlayerClassData;
        PlayerClassDataSO localData = LocalDataAccess.Instance.Game.GetClass(_classId);

        Debug.Log($"[Compare] PlayerStats 데이터 이름: {statsData.name}");
        Debug.Log($"[Compare] LocalDataAccess 데이터 이름: {localData.name}");

        Debug.Log($"[Compare] 같은 SO 인스턴스인가? {ReferenceEquals(statsData, localData)}");

        Debug.Log($"[Compare] PlayerStats 체력: {statsData.MaxHealth}");
        Debug.Log($"[Compare] LocalDataAccess 체력: {localData.MaxHealth}");
    }
}