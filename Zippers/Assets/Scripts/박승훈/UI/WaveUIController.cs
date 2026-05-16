using System;
using TMPro;
using UnityEngine;

public class WaveUIController : MonoBehaviour
{
    [Header("웨이브 정보")]
    [SerializeField] private int _maxWave;
    public int MaxWave => _maxWave;
    [SerializeField] private int _currentWave;
    public int CurrentWave => _currentWave;
    [SerializeField] private int _leftzombieCount;
    public int LeftZombieCount => _leftzombieCount;
    
    [Header("UI 컴포넌트")]
    [SerializeField] private TMP_Text _maxWaveText;
    [SerializeField] private TMP_Text _currentWaveText;
    [SerializeField] private TMP_Text _leftZombieCount;

    public void SetMaxWaveText(int maxWave)
        => _maxWaveText.text = maxWave.ToString();

    public void SetCurrentWaveText(int currentWave)
        => _currentWaveText.text = currentWave.ToString();

    public void SetLeftZombieCount(int leftZombieCount)
        => _leftZombieCount.text = leftZombieCount.ToString();
}