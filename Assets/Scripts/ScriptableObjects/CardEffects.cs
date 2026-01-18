using UnityEngine;

[CreateAssetMenu(fileName = "CardEffects", menuName = "Scriptable Objects/New Card Effect")]
public class CardEffects : ScriptableObject
{
    [Header("Rotation")]
    public float rotationSpeed;
    public Vector2 mouseRotationMulti;
    public Vector2 autoRotationMulti;

    [Header("Animations")]
    public float attackDistance;
    public float attackDuration;
    public float dieDuration;
    public Vector3 damagedTextScale;

    [Header("Sounds")]
    public AudioClip clipTurn;
    public AudioClip clipMove;
    public AudioClip clipAttack;
    public AudioClip clipDie;
    public AudioClip clipHurt;
}
