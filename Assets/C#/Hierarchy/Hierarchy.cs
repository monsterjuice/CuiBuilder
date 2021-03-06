﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Hierarchy : MonoBehaviour, IPoolHandler, IPointerClickHandler, ISelectHandler
{
    public delegate void OnChildrenEvent(Hierarchy item);

    public static Dictionary<GameObject, Hierarchy> Lookup = new Dictionary<GameObject, Hierarchy>();
    private static List<Hierarchy> history = new List<Hierarchy>();
    private readonly List<Hierarchy> Children = new List<Hierarchy>();

    private static Dictionary<string, Hierarchy> Root;

    public event OnChildrenEvent OnChildAdded;
    public event OnChildrenEvent OnChildRemoved;

    [SerializeField] private Hierarchy parent;

    public List<Hierarchy> GetChildren()
    {
        return Children.ToList();
    }

    public int GetChildrenCount()
    {
        return Children.Count;
    }

    public static List<Hierarchy> Selection { get { return HierarchyView.GetSelectedItems().Select( o => Lookup[ o ] ).ToList(); } }
    private void Awake()
    {
        if (Root == null)
        {
            Root = new Dictionary<string, Hierarchy>()
            {
                { "Hud", GameObject.Find("Hud").GetComponent<Hierarchy>() },
                { "Overlay", GameObject.Find("Overlay").GetComponent<Hierarchy>() }
            };
        }
        Lookup[ gameObject ] = this;
    }

    private void Start()
    {
        Init();
    }

    public static Hierarchy FindByName(string name)
    {
        return history.LastOrDefault(p => p.name == name);
    }

    private void Init()
    {
        foreach (Transform childT in transform)
        {
            if (childT.tag == "CUI")
            {
                var child = Lookup[ childT.gameObject ];
                child.parent = this;
                AddChild( child );
            }
        }
    }

    public void AddChild(Hierarchy child)
    {
        if (!Children.Contains( child ))
        {
            Children.Add( child );
            OnChildrenUpdated();
            if (OnChildAdded != null) OnChildAdded.Invoke( child );
        }

    }

    public void RemoveChild(Hierarchy child)
    {
        if (Children.Contains(child))
        {
            Children.Remove(child);
            OnChildrenUpdated();
            if (OnChildRemoved != null) OnChildRemoved.Invoke(child);
        }
    }

    private void OnChildrenUpdated()
    {
        Children.Sort( ( x, y ) => x.transform.GetSiblingIndex().CompareTo( y.transform.GetSiblingIndex() ) );
    }

    public void OnPoolEnter()
    {
        foreach (var child in Children.ToList())
        {
            PoolManager.Release(PrefabType.Cui, child.gameObject);
        }
        Lookup.Remove( gameObject );
        history.Remove( this );
        HierarchyView.Remove(gameObject);
        if (parent != null) parent.RemoveChild(this);
        Children.Clear();
        OnChildAdded = null;
        OnChildRemoved = null;
        parent = null;
    }

    public void OnPoolLeave()
    {
        Lookup[ gameObject ] = this;
        history.Add(this);
        Init();
    }

    public void SetParent( Hierarchy newParent )
    {
        var rTransform = (RectTransform) transform;
        if (parent != null)
        {

            var position = rTransform.GetParent().GetWorldPoint( rTransform.anchorMin );
            var size = rTransform.GetSizeWorld();
            transform.SetParent( newParent.transform, false );
            parent.RemoveChild( this );
            rTransform.SetPositionAnchorLocal( rTransform.GetParent().GetLocalPoint( position ) );
            rTransform.SetSizeWorld( size );
        }
        else
        {
            transform.SetParent( newParent.transform, false );
        }
        parent = newParent;
        parent.AddChild( this );
        if (!HierarchyView.IsCreated(gameObject))
            HierarchyView.AddChild( gameObject );
        HierarchyView.ChangeParent( parent.gameObject, gameObject );
    }

    public void SetParent(string parentName)
    {
        Hierarchy parent;
        if (Root.TryGetValue(parentName, out parent))
        {
            SetParent(parent);
            return;
        }
        parent = history.LastOrDefault(hierarchy => hierarchy.name == parentName);
        if (parent != null)
        {
            SetParent(parent);
        }
    }

    public void SetParent( GameObject newParent )
    {
        SetParent( Lookup[ newParent ] );
    }
    public void OnPointerClick( PointerEventData eventData )
    {
        if (HierarchyView.Select( gameObject )) return;
    }

    public void OnSelected()
    {

    }

    public void OnUnselected()
    {

    }
}
