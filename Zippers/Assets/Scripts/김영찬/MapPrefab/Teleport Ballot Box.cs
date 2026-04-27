using System;
using UnityEngine;

public class TeleportBallotBox : MonoBehaviour
{
    [SerializeField] Collider _collider;

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
    
}
