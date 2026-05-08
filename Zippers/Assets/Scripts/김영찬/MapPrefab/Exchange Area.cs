using System;
using UnityEngine;

public class ExchangeArea : MonoBehaviour
{
    [SerializeField] LayerMask _unitLayer;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == _unitLayer)
        {
            // ToDo : 상점 UI 활성화
            // 플레이어 개인을 식별해야됨
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == _unitLayer)
        {
            // ToDo : 상점 UI 비활성화
            // 플레이어 개인을 식별해야됨
        }
    }
}
