/// <summary>
/// Shop Node Map Controller
/// </summary>
public class ShopTypeMapController : MapController
{
    public override NodeType NodeType => NodeType.Shop;
    
    protected override void Start()
    {
        base.Start();
        DebugTool.Log($"{gameObject.name} Auto DeActive", DebugType.Node, this);
        gameObject.SetActive(false);
    }
}
