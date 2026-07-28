using UnityEngine;
using System;
using System.Collections;
using TMPro;
public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;
    [SerializeField] private GameObject _damageTextPrefab;
    [SerializeField] private Transform _effectCanvas;
    [SerializeField] private float _damageTextEffectTime = 1.0f;
    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public IEnumerator CreateDamageTextCoroutine(int damage , Vector3 targetPosition)
    {
        if(_damageTextPrefab == null || _effectCanvas == null) yield break; 

        Vector3 worldPosition = targetPosition + Vector3.up;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        GameObject spawnObject = Instantiate(_damageTextPrefab,screenPosition,Quaternion.identity);
        if (_effectCanvas != null)
        {
            spawnObject.transform.SetParent(_effectCanvas, true);
        }
        TextMeshProUGUI spawnDamageText = spawnObject.GetComponent<TextMeshProUGUI>();
        if(spawnDamageText != null)
        {
            spawnDamageText.text = damage.ToString();
        }

        float timer = 0f;
        while(timer < _damageTextEffectTime)
        {
            timer += Time.deltaTime;
            spawnObject.transform.Translate(Vector3.up * Time.deltaTime * 5f);
            yield return null;
        }
        Destroy(spawnObject);
    }
}
