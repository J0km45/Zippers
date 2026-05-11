using UnityEngine;
using System.Collections.Generic;

public class PlayerUpgradeProvider : MonoBehaviour
{
    public List<UpgradeEntry> AbleUpgrade(WeaponType weaponType)
    {
        List<UpgradeEntry> result = new();

        if (LocalDataAccess.Instance == null)
        {
            return result;
        }

        ClassUpgradeData upgradeData = LocalDataAccess.Instance.Game.GetUpgrade(weaponType);

        if (upgradeData == null)
        {
            return result;
        }

        AddClassUpdrage(result, upgradeData);
        AddCommonUpgrade(result, upgradeData);

        return result;
    }
    //랜덤 뽑기
    public List<UpgradeEntry> RandomUpgrade(WeaponType weaponType, PlayerIngameData playerIngameData, int count = 4)
    {
        List<UpgradeEntry> UpgradeList = AbleUpgrade(weaponType);
        List<UpgradeEntry> filterList = new();

        foreach (UpgradeEntry entry in UpgradeList)
        {
            if (entry == null)
            {
                continue;
            }
            if (playerIngameData != null && playerIngameData.GetLevel(entry) >= entry.MaxLevel)
            {
                continue;
            }

            filterList.Add(entry);
        }
        Shuffle(filterList);

        int resultCount = Mathf.Min(count, filterList.Count);
        List<UpgradeEntry> result = new();

        for (int i = 0; i < resultCount; i++)
        {
            result.Add(filterList[i]);
        }

        return result;
    }

    private void AddCommonUpgrade(List<UpgradeEntry> result, ClassUpgradeData data)
    {
        UpgradeEnable(result, data.MaxHealth);
        UpgradeEnable(result, data.Stamina);
        UpgradeEnable(result, data.StaminaRegen);
        UpgradeEnable(result, data.Damage);
        UpgradeEnable(result, data.AttackSpeed);
        UpgradeEnable(result, data.MoveSpeed);
    }

    private void AddClassUpdrage(List<UpgradeEntry> result, ClassUpgradeData data)
    {
        switch (data)
        {
            case MeleeUpgradeData melee:
                UpgradeEnable(result, melee.DamageReduction);
                UpgradeEnable(result, melee.SprintSpeed);
                break;

            case RifleUpgradeData rifle:
                UpgradeEnable(result, rifle.MagazineCapacity);
                UpgradeEnable(result, rifle.ReloadTime);
                UpgradeEnable(result, rifle.PierceCount);
                UpgradeEnable(result, rifle.BulletDistance);
                break;
            case ShotgunUpgradeData shotgun:
                UpgradeEnable(result, shotgun.MagazineCapacity);
                UpgradeEnable(result, shotgun.ReloadTime);
                UpgradeEnable(result, shotgun.KnockbackPower);
                UpgradeEnable(result, shotgun.ProjectileCount);
                break;

            case PistolUpgradeData pistol:
                UpgradeEnable(result, pistol.MagazineCapacity);
                UpgradeEnable(result, pistol.ReloadTime);
                UpgradeEnable(result, pistol.SightRange);
                UpgradeEnable(result, pistol.CollectRange);
                break;
        }
    }

    private void UpgradeEnable(List<UpgradeEntry> result, UpgradeEntry entry)
    {
        if (entry == null)
        {
            return;
        }
        if (!entry.IsEnabled)
        {
            return;
        }
        result.Add(entry);
    }

    //업그레이드 리스트 섞기
    private void Shuffle(List<UpgradeEntry> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int ramdonIndex = Random.Range(0, i + 1);

            UpgradeEntry temp = list[i];
            list[i] = list[ramdonIndex];
            list[ramdonIndex] = temp;
        }
    }

}
// 플레이어 오브젝트에 넣어서 사용하기.