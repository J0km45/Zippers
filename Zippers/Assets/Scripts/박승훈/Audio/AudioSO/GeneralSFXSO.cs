using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GeneralSFXSO", menuName = "Zippers/SO/Audio/GeneralSFXSO", order = 3)]
public class GeneralSFXSO : ScriptableObject
{
    [Header("캐릭터 SFX 목록")]
    
    [Space(5)][Header("Drop Scrap")][Tooltip("Walking Clip 필요")]
    [SerializeField] private List<AudioClip> walkingSfx = new List<AudioClip>();
    public List<AudioClip> WalkingSfx => walkingSfx;
}
