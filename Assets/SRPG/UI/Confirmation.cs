using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System;
public class Confirmation : MonoBehaviour
{

    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;
    [SerializeField]private TextMeshProUGUI _messageText;
    public Action YesAction;
    public Action NoAction;
    void Awake()
    {
        _yesButton.onClick.AddListener(YesClick);
        _noButton.onClick.AddListener(NoClick);
    }
    public void SetUp(string message)
    {
        _messageText.text = message;
    }
    public void YesClick()
    {
        YesAction?.Invoke();
    }
    public void NoClick()
    {
        NoAction?.Invoke();
    }
    void OnDisable()
    {
        YesAction = null;
        NoAction = null;
    }
}
