using UnityEngine;

[CreateAssetMenu(fileName = "HandShape", menuName = "Scriptable Objects/HandShape_Preset")]
public class HandShape_Preset : ScriptableObject
{
    public int cardDistance;
    public AnimationCurve handPositionCurve = new AnimationCurve();
    public float handHeightOffset;
    public AnimationCurve handRotationCurve = new AnimationCurve();
    public float handRotationOffset;
}
