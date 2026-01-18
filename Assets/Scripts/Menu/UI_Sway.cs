using UnityEngine;
using DG.Tweening;

public class UI_Sway : MonoBehaviour
{
    [SerializeField] float angleRange;
    [SerializeField] float timeMin;
    [SerializeField] float timeMax;
    void Start()
    {
        RectTransform rT = GetComponent<RectTransform>();
        float startRotAdd = Random.Range(-angleRange, angleRange);

        Vector3 startRot = rT.rotation.eulerAngles;
        Vector3 finalRot = rT.rotation.eulerAngles;

        startRot.z -= startRotAdd;
        finalRot.z += startRotAdd;

        rT.rotation = Quaternion.Euler(startRot);
        rT.DORotate(finalRot, Random.Range(timeMin, timeMax)).SetLoops(-1, LoopType.Yoyo);
    }
}
