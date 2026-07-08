using UnityEngine;
using UnityEngine.UI;
using System;
public class ReturnButton : MonoBehaviour
{
    public Action ReturnAction;
    [SerializeField] private Button _returnButton;
    void Awake()
    {
        _returnButton.onClick.AddListener(Return);
    }
    public void Return()
    {
        ReturnAction?.Invoke();
    }
    private void OnDisable()
    {
        ReturnAction = null;
    }
}
