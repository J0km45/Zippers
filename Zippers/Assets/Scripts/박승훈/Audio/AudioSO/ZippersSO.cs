using System;
using UnityEngine;

public abstract class ZippersSO : ScriptableObject
{
    public void OnEnable()
        => DictionaryInit();

    protected abstract void DictionaryInit();
}