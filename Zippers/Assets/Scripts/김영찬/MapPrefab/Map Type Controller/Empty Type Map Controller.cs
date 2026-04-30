/// <summary>
/// Empty Node Map Controller
/// </summary>
public class EmptyTypeMapController : MapController
{
    public override NodeType NodeType => NodeType.Empty;
    
    protected override void Start()
    {
        base.Start();
        DebugTool.Log($"{gameObject.name} Auto DeActive", DebugType.Node, this);
        gameObject.SetActive(false);
    }
}
