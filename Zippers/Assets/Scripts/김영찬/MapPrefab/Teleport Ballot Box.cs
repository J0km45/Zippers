using System;
using UnityEngine;

public class TeleportBallotBox : MonoBehaviour
{
    [SerializeField] BoxCollider _collider;

    private int _votePlayer;
    public event Action<int> OnVoteChange;
    
    private void OnTriggerEnter(Collider other)
    {
        if(_votePlayer >= 4) return;
        _votePlayer++;
        OnVoteChange?.Invoke(_votePlayer);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(_votePlayer <= 0) return;
        _votePlayer--;
        OnVoteChange?.Invoke(_votePlayer);
    }
    
    private void OnDrawGizmos()
    {
        if(_collider == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_collider.center, _collider.size);
    }
    
}
