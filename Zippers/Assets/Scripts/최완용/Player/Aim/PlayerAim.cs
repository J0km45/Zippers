using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    public bool IsAiming { get; private set; }

    public void SetAiming(bool aiming)
    {
        IsAiming = aiming;
        Debug.Log($"[PlayerAim] Aiming 상태: {IsAiming}");
    }
}
