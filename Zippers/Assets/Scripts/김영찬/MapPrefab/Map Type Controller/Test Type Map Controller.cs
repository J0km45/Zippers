/// <summary>
/// Test Node Map Controller
/// </summary>
public class TestTypeMapController : MapController
{
    public override NodeType NodeType => NodeType.Test;
    
    protected override void Start()
    {
        base.Start();
        DebugTool.Log($"{gameObject.name} Auto DeActive", DebugType.Node, this);
        gameObject.SetActive(false);
    }
}
