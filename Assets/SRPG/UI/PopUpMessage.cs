using UnityEngine;
using TMPro;
using System.Collections;
public class PopUpMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _popUpText;
    [SerializeField] private float _popUpDuration = 2.0f;
    public void PopUp(string message)
    {
        StartPopUp(message);
    }
    private void StartPopUp(string text)
    {
        _popUpText.text = text;
        StopCoroutine(nameof(WaitPopUp));
        StartCoroutine(nameof(WaitPopUp));
    }

    private IEnumerator WaitPopUp()
    {
        yield return new WaitForSeconds(_popUpDuration);
        gameObject.SetActive(false);
    }
}
