using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System;
public class LeftStatusPanel : MonoBehaviour
{
    [SerializeField] private Image _backImage; 
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _hp;
    [SerializeField] private TextMeshProUGUI _atk;
    [SerializeField] private TextMeshProUGUI _def;
    [SerializeField] private TextMeshProUGUI _matk;
    [SerializeField] private TextMeshProUGUI _mdef;
    [SerializeField] private TextMeshProUGUI _spd;
    [SerializeField] private TextMeshProUGUI _mov;
    public bool LockChenge;
    public void SetUp(IMapObject setObject)
    {
        if(LockChenge) return;
        if(setObject == null)
        {
            _backImage.gameObject.SetActive(false);
            return;
        }
        if(setObject is UnitBase unit)
        {
            _name.text = unit.Name;
            _hp.text = $"HP{unit.Hp}/{unit.MaxHp}";
            _atk.text = $"ATK{unit.Atk}";
            _def.text = $"DEF{unit.Def}";
            _matk.text = $"MATK{unit.MAtk}";
            _mdef.text = $"MDEF{unit.MDef}";
            _spd.text = $"SPD{unit.Spd}";
            _mov.text = $"MOV{unit.Mov}";
        }
        else if(setObject is GimmickBase gimmick)
        {
            _name.text = gimmick.Name;
            _hp.text = (gimmick.IsAttackable) ? $"HP{gimmick.Hp}/{gimmick.MaxHp}" : "";
            _atk.text = "";
            _def.text = "";
            _matk.text = "";
            _mdef.text = "";
            _spd.text = "";
            _mov.text = "";
        }
    }
    public void SetStatusActive(bool active)
    {
        if(LockChenge)return;
        _backImage.gameObject.SetActive(active);
    }
}
