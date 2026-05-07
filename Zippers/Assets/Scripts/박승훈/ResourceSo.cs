using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourcesSO", menuName = "Zippers/Resources", order = 99)]
public class ResourceSo : ZippersSO
{
    [Header("스크랩")] [Tooltip("개인 재화 프리펩")] [Space(5)] [SerializeField]
    private GameObject scrap;

    public GameObject Scrap => scrap;

    [Header("보급품")] [Tooltip("팀 재화 프리펩")] [Space(5)] [SerializeField]
    private GameObject supplies;

    public GameObject Supplies => supplies;

    [Header("감염 샘플")] [Tooltip("메타 재화 프리펩")] [Space(5)] [SerializeField]
    private GameObject infectionSample;
    public GameObject InfectionSample => infectionSample;
    
    public GameObject GetResource(ResourcesType type)
    {
        switch (type)
        {
            case ResourcesType.None:
                return null;
            case ResourcesType.Scrap:
                return Scrap;
            case ResourcesType.Supplies:
                return Supplies;
            case ResourcesType.InfectionSample:
                return InfectionSample;
        }
        
        return null;
    }

    protected override void DictionaryInit() { }
}