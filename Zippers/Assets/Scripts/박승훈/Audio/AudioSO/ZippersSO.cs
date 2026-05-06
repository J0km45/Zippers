using System;
using UnityEngine;

public abstract class ZippersSO : ScriptableObject
{
    public void OnEnable()
    {
        if (!Application.isPlaying)
            return;
        
        DictionaryInit();
    }

    protected abstract void DictionaryInit();
}