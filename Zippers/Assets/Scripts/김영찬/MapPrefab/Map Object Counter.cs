using System;
using UnityEngine;

public class MapObjectCounter : MonoBehaviour
{
    [SerializeField] private Collider _countArea;
    [SerializeField] private MapController _controller;
    [SerializeField] LayerMask _unitLayer;
    [SerializeField] LayerMask _monsterLayer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == _unitLayer)
        {
            _controller.Data.PlusAlivePlayerCount();
        }

        if (other.gameObject.layer == _monsterLayer)
        {
            _controller.Data.PlusAliveMonsterCount();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == _unitLayer)
        {
            _controller.Data.MinusAlivePlayerCount();
        }
        
        if (other.gameObject.layer == _monsterLayer)
        {
            _controller.Data.MinusAliveMonsterCount();
        }
    }
}
