﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableData : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    protected virtual void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (autoUpdate)
            {
                UnityEditor.EditorApplication.update += NotifyOfUpdatesValues;
            }
        }
    }

    public virtual void NotifyOfUpdatesValues()
    {
        UnityEditor.EditorApplication.update -= NotifyOfUpdatesValues;
        if (OnValuesUpdated != null)
            OnValuesUpdated();
    }
}
