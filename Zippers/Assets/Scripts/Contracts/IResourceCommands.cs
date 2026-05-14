namespace Zippers.Network.Contracts
{
    /// <summary>
    /// 외부 시스템(몬스터/아이템 픽업/노드 클리어 보상 등) 이 재화 획득·차감을 요청하는
    /// 서버 엔드포인트.
    ///
    /// 클라이언트는 직접 호출하지 않고 ServerRpc 를 통해 호스트가 호출하는 경로.
    ///
    /// 구현체:
    ///   - PlayerEconomyNetState (per-player, Scrap / InfectionSample)
    ///   - TeamEconomyNetState   (team singleton, Supplies)
    ///
    /// 사용처:
    ///   - 몬스터 사망 보상, 아이템 픽업, 노드 클리어 보상
    ///   - 업그레이드 구매 (내부적으로 ServerSpendResource 호출)
    ///   - 디버그/치트 UI
    ///
    /// 반환값: 호스트에서만 의미. 성공 시 true.
    /// 클라이언트가 호출하면 무조건 false 반환 (안전망).
    /// </summary>
    public interface IResourceCommands
    {
        bool ServerGrantResource(ulong clientId, ResourcesType type, float amount, string source);
        bool ServerSpendResource(ulong clientId, ResourcesType type, float amount, string reason);
    }
}
