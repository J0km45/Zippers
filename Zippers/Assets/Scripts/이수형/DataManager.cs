using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DataManager : MonoBehaviour
{

    [Header("Player Class")]
    public SheetData _classSheet;
    [SerializeField] private List<PlayerClassDataSO> _classDataList;
    private Dictionary<int, PlayerClassDataSO> _classDataDictionary = new();

    [Header("Zombie Stat")]
    public SheetData _zombieStatSheet;
    [SerializeField] private List<ZombieStatSO> _zombieStatDataList;
    private Dictionary<int, ZombieStatSO> _zombieStatDataDictionary = new();

    [Header("Wave Info")]
    public SheetData _waveInfoSheet;
    [SerializeField] private List<WaveInfoSO> _waveInfoDataList;
    private Dictionary<int, WaveInfoSO> _waveInfoDataDictionary = new();

    private void Awake()
    {
        _classDataDictionary = InitDict(_classDataList);
        _zombieStatDataDictionary = InitDict(_zombieStatDataList);
        _waveInfoDataDictionary = InitDict(_waveInfoDataList);
    }

    private void Start()
    {
        LoadSheetData(_classSheet, _classDataList, _classDataDictionary);
        LoadSheetData(_zombieStatSheet, _zombieStatDataList, _zombieStatDataDictionary);
        LoadSheetData(_waveInfoSheet, _waveInfoDataList, _waveInfoDataDictionary);
    }


    private Dictionary<int, T> InitDict<T>(List<T> list)
        where T : ScriptableObject, ISheetParsable
    {
        if (list == null || list.Count == 0)
        {
            DebugTool.Warning(
                $"[{typeof(T).Name}] 리스트 비어있음 - 빈 사전 반환",
                DebugType.Data, this);
            return new Dictionary<int, T>();
        }

        return list.ToDictionary(x => x.Id);
    }


    private void LoadSheetData<T>(
        SheetData sheet,
        List<T> list,
        Dictionary<int, T> dict,
        int headerRowCount = 1
    ) where T : ScriptableObject, ISheetParsable
    {
        StartCoroutine(sheet.Load((split, lines) =>
        {
            if (lines == null)
            {
                DebugTool.Error(
                    $"[{typeof(T).Name}] 시트 로드 실패 - lines가 null",
                    DebugType.Data, this);
                return;
            }

            for (int i = headerRowCount; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] cols = line.Split(split);

                if (cols.Length == 0 || !int.TryParse(cols[0], out int id))
                {
                    DebugTool.Error(
                        $"[{typeof(T).Name}] {i}번째 줄 ID 파싱 실패: '{(cols.Length > 0 ? cols[0] : "(empty)")}'",
                        DebugType.Data, this);
                    continue;
                }

                T data;
                if (dict.TryGetValue(id, out var existing))
                {
                    // 사전에 이미 있는 경우엔 기존 SO를 갱신
                    data = existing;
                }
                else
                {
                    // 사전에 없으면 임시 인스턴스 생성하여 추가
                    data = ScriptableObject.CreateInstance<T>();
                    data.name = $"{typeof(T).Name}_{id}";
                    dict.Add(id, data);
                    list.Add(data);
                    DebugTool.Warning(
                        $"[{typeof(T).Name}] ID {id} 사전에 없어서 새 인스턴스 생성",
                        DebugType.Data, this);
                }

                data.SetData(cols);
            }

            DebugTool.Log(
                $"[{typeof(T).Name}] 시트 로드 완료 (총 {dict.Count}건)",
                DebugType.Data, this);
        }));
    }
}
