using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSFXSO", menuName = "Data/Audio/WeaponSFXSO", order = 1)]
public class WeaponSFXSO : ScriptableObject
{
    [Header("무기 SFX 목록")]
    
    [Space(5)][Header("근접무기 공격 SFX")]
    [SerializeField] private List<AudioClip> meleeClips =  new List<AudioClip>();
    public List<AudioClip> MeleeClips =>  meleeClips;
    
    [Space(5)][Header("라이플 공격 SFX")]
    [SerializeField] private List<AudioClip> rifleClips =  new List<AudioClip>();
    public List<AudioClip> RifleClips => rifleClips;
    
    [Space(5)][Header("샷건 공격 SFX")]
    [SerializeField] private List<AudioClip> shotGunClips =  new List<AudioClip>();
    public List<AudioClip> ShotgunClips => shotGunClips;
    
    [Space(5)][Header("권총 공격 SFX")]
    [SerializeField] private List<AudioClip> pistolClips =  new List<AudioClip>();

    public List<AudioClip> PistolClips => pistolClips;

    [Space(5)] [Header("라이플 재장전 SFX")]
    [SerializeField] private AudioClip rifleReloadClip;
    public AudioClip RifleReloadClip => rifleReloadClip;
    
    [Space(5)][Header("샷건 재장전 SFX")]
    [SerializeField] private AudioClip shotgunReloadClip;
    public AudioClip ShotgunReloadClip => shotgunReloadClip;

    [Space(5)] [Header("권총 재장전 SFX")]
    [SerializeField] private AudioClip pistolReloadClip;
    public AudioClip PistolReloadClip => pistolReloadClip;

    private Dictionary<WeaponType, List<AudioClip>> _weaponSfxDict = new Dictionary<WeaponType, List<AudioClip>>();
    public Dictionary<WeaponType, List<AudioClip>> WeaponSfxDict => _weaponSfxDict;

    private Dictionary<WeaponType, AudioClip> _reloadSfxDict = new Dictionary<WeaponType, AudioClip>();
    public Dictionary<WeaponType, AudioClip> ReloadSfxDict => _reloadSfxDict;

    public void WeaponSfxInit()
    {
        _weaponSfxDict.Clear();
        
        _weaponSfxDict.Add(WeaponType.Melee, meleeClips);
        _weaponSfxDict.Add(WeaponType.Rifle, rifleClips);
        _weaponSfxDict.Add(WeaponType.Shotgun, ShotgunClips);
        _weaponSfxDict.Add(WeaponType.Util, pistolClips);
    }

    public void ReloadSfxInit()
    {
        _reloadSfxDict.Clear();
        
        _reloadSfxDict.Add(WeaponType.Rifle, rifleReloadClip);
        _reloadSfxDict.Add(WeaponType.Shotgun, shotgunReloadClip);
        _reloadSfxDict.Add(WeaponType.Util, pistolReloadClip);
    }
}
