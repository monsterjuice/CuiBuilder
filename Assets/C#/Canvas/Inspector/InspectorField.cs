﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class InspectorField : MonoBehaviour
{
    public string Name;
    [Serializable]
    public class OnInspectorFieldChangedEvent : UnityEvent<object>
    { }
    public abstract object GetValue();
    public abstract void SetValue(object value);

    [SerializeField]
    protected OnInspectorFieldChangedEvent onChanged;

    public virtual void Init(object value)
    { }

    public virtual void AddListener(UnityAction<object> callback)
    {
        this.onChanged.AddListener(callback);
    }

    public void SetLabel(string text)
    {
        GetComponentInChildren<Text>().text = text;
    }
}