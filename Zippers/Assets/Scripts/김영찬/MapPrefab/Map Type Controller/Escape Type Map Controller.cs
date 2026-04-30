/// <summary>
/// Escape Node Map Controller
/// </summary>
public class EscapeTypeMapController : MapController
{
    public override NodeType NodeType => NodeType.Escape;
    
    protected override void Start()
    {
        base.Start();
        DebugTool.Log($"{gameObject.name} Auto DeActive", DebugType.Node, this);
        gameObject.SetActive(false);
    }
}
