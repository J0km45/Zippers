using System;
using Unity.Netcode;

namespace Zippers.Network.Contracts
{
    /// <summary>
    /// 업그레이드 ID와 레벨을 묶은 NetworkList 항목.
    ///
    /// 사용처:
    ///   - PlayerEconomyNetState.personalUpgradeLevels (per-player)
    ///   - TeamEconomyNetState.teamUpgradeLevels (team singleton)
    ///
    /// NetworkList&lt;T&gt; 요소는 INetworkSerializable + IEquatable&lt;T&gt; 가 필수.
    /// </summary>
    public struct UpgradeLevelEntry : INetworkSerializable, IEquatable<UpgradeLevelEntry>
    {
        public int UpgradeId;
        public int Level;

        public UpgradeLevelEntry(int upgradeId, int level)
        {
            UpgradeId = upgradeId;
            Level = level;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref UpgradeId);
            serializer.SerializeValue(ref Level);
        }

        public bool Equals(UpgradeLevelEntry other)
        {
            return UpgradeId == other.UpgradeId && Level == other.Level;
        }

        public override bool Equals(object obj)
        {
            return obj is UpgradeLevelEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            // System.HashCode.Combine 은 Unity 2021.2+ / .NET Standard 2.1 필요.
            // 호환을 위해 수동 조합.
            unchecked
            {
                return (UpgradeId * 397) ^ Level;
            }
        }

        public override string ToString()
        {
            return $"UpgradeLevelEntry(Id={UpgradeId}, Level={Level})";
        }
    }
}
