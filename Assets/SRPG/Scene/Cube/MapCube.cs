using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class MapCube : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Vector2Int Position;//マネージャーの方から設定する
    private List<Material> _materials = new List<Material>();
    public Color DefaultColor = Color.white;
    public static Action<MapCube> EnterAction;
    public static Action<MapCube> ExitAction;
    public static Action<MapCube> ClickAction;
    public IMapObject CurrentObject { get; set; }
    public UnitBase CurrentUnit => CurrentObject as UnitBase;
    public GimmickBase CurrentGimmick => CurrentObject as GimmickBase;
    public bool LockColor = false;
    [field: SerializeField] public int MapCost{get;private set;} = 1;
    public Color CurrentColor
    {
        get =>_materials.Count > 0 ? _materials[0].color : DefaultColor;

        set
        {
            if(LockColor) return;
            foreach(Material material in _materials)
            {
                material.color = value;
            }
        }
    }
    void Awake()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in renderers)
        {
            if (meshRenderer.material != null)
            {
                _materials.Add(meshRenderer.material);
            }
        }
        if(_materials.Count > 0) DefaultColor = _materials[0].color;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        EnterAction?.Invoke(this);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        ExitAction?.Invoke(this);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        ClickAction?.Invoke(this);
    }
    private void OnDestroy()
    {
        foreach(Material material in _materials)
        {
            if(material != null)
            {
                Destroy(material);
            }
        }
        _materials.Clear();
    }
}
