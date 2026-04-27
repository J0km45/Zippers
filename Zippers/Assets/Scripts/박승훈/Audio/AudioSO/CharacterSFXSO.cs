using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터 관련 SFX를 관리하기 위한 SO
/// </summary>
[CreateAssetMenu(fileName = "CharacterSFXSO", menuName = "Data/Audio/CharacterSFXSO", order = 0)]
public class CharacterSFXSO : ScriptableObject
{
    [Header("캐릭터 SFX 목록")]
    
    [Space(5)][Header("Walking")][Tooltip("Walking Clip 필요")]
    [SerializeField] private List<AudioClip> walkingSfx = new List<AudioClip>();
    public List<AudioClip> WalkingSfx => walkingSfx;
    
    [Space(5)][Header("Running")][Tooltip("Running Clip 필요")]
    [SerializeField] private List<AudioClip> runningSfx = new List<AudioClip>();
    public List<AudioClip> RunningSfx => runningSfx;
    
    [Space(5)][Header("Hit")][Tooltip("Running Clip 필요")]
    [SerializeField] private List<AudioClip> hitSfx = new List<AudioClip>();
    public List<AudioClip> HitSfx => hitSfx;
    
    [Space(5)][Header("Death")][Tooltip("Death Clip 필요")]
    [SerializeField] private List<AudioClip> deathSfx = new List<AudioClip>();
    public List<AudioClip> DeathSfx => deathSfx;
}
