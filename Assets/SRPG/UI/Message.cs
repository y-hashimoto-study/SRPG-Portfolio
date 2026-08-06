using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;
using System;public class Message : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI _messageText;
    [SerializeField]private Image _image;
    private Action _completeAction;
    [SerializeField] private float messageTime = 1.0f;
    public void SetUp(string message ,Sprite itemSprite,Action completeAction)
    {
        if(message != null) _messageText.text = message;
        if(itemSprite != null) _image.sprite = itemSprite;
        if(completeAction != null) _completeAction = completeAction;

        StopAllCoroutines();
        StartCoroutine(WaitAndAutoClose(messageTime));
    }

    public IEnumerator WaitAndAutoClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        _completeAction?.Invoke();
    }

}
