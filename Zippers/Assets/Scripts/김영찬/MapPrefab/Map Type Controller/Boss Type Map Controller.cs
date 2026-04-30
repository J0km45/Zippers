/// <summary>
/// Boss Node Map Controller
/// </summary>
public class BossTypeMapController : MapController
{
    public override NodeType NodeType => NodeType.Boss;
    
    protected override void Start()
    {
        base.Start();
        DebugTool.Log($"{gameObject.name} Auto DeActive", DebugType.Node, this);
        gameObject.SetActive(false);
    }
}
