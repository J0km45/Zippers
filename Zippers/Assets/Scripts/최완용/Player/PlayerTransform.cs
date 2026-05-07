using UnityEngine;

public class PlayerTransform : MonoBehaviour
{
    private void OnEnable()
    {
        PlayerTransformList.Instance.AddPlayer(transform);
    }

    private void OnDisable()
    {
        if(PlayerTransformList.Instance == null)
        {
            return;
        }

        PlayerTransformList.Instance.DeletePlayer(transform);
    }
}
