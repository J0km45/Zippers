using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataManager : MonoBehaviour
{
    public SheetData _classSheet;
    [SerializeField] private List<PlayerClassDataSO> _classDataList;
    private Dictionary<string, PlayerClassDataSO> _classDataDictionary = new();//string은 파일 이름 기준

    private void Awake() => InitClassDataDictionary();


    private void Start() => StartCoroutine(_classSheet.Load(SetClassDatas));
    

    public void SetClassDatas(char splitSymbol, string[] lines)
    {
        if(lines == null)
        {
            Debug.LogError("Failed to load class data from sheet.");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            // 나누는 문자열 기준(TSV인지? CSV인지?) 다시 문자열 배열로 쪼개서
            string[] cols = lines[i].Split(splitSymbol);

            PlayerClassDataSO classData;

            if (_classDataDictionary.ContainsKey(cols[1]))
            {
                // 딕셔너리에 SO가 이미 추가되어있는 경우엔 가져다 씀
                classData = _classDataDictionary[cols[1]];

            }
            else
            {
                classData = ScriptableObject.CreateInstance<PlayerClassDataSO>();
                classData.name = cols[1];
                _classDataDictionary.Add(cols[1], classData);
                Debug.LogWarning($"Class data for {cols[1]} not found in dictionary. Created new SO instance and added to dictionary.");
                // 임시로 만들어서 추가
                _classDataList.Add(classData);
            }

            classData.SetData(cols);
        }
        // 모든 몬스터에 대해 수행해줘야 함

        // 몬스터 데이터에 담아주기
        
    }
    private void InitClassDataDictionary()
    {
        if (_classDataList == null || _classDataList.Count == 0)
        {
            Debug.LogError("Class data list is null or empty. Cannot initialize class data dictionary.");
            return;
        }
        _classDataDictionary = _classDataList.ToDictionary(data => data.WeaponType.ToString());
        //_classDataList.Clear();
        //_classDataList = null;
    }
}
