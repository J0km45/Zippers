using Unity.VisualScripting;

/// <summary>
/// Battle Node Map Controller
/// </summary>
public class BattleTypeMapController : MapController
{
    public override NodeType NodeType => NodeType.Battle;

    protected override void Start()
    {
        base.Start();
        DebugTool.Log($"{gameObject.name} Auto DeActive", DebugType.Node, this);
        gameObject.SetActive(false);
    }
}
