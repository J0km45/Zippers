using System;
using UnityEngine;

public class TeleportBallotBox : MonoBehaviour
{
    private int _votePlayer;
    public event Action<int> OnVoteChange;

    private void OnEnable()
    {
        _votePlayer = 0;
        OnVoteChange?.Invoke(_votePlayer);
    }

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
