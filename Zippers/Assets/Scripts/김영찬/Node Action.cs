using System;
using UnityEngine;

/// <summary>
/// 노드의 state에 따른 Action을 정의하는 추상 클래스
/// </summary>
public abstract class NodeAction : MonoBehaviour
{
    public abstract void EnterReadyState();
    public abstract void RunningReadyState();
    public abstract void ExitReadyState();
    public abstract void EnterBattleState();
    public abstract void RunningBattleState();
    public abstract void ExitBattleState();
    public abstract void EnterClearState();
}
