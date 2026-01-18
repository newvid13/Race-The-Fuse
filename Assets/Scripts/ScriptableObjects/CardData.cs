using UnityEngine;

[CreateAssetMenu(fileName = "CardUntitled", menuName = "New Card/Create Card")]
public class CardData : ScriptableObject
{
    public int pickWeight;

    [Header ("Visuals")]
    public Vector2 dimensions;
    public string cardName;
    public Sprite frontFace;
    public Sprite backFace;
    public Sprite pictureFrame;

    [Header("Attack")]
    public bool showAttack;
    public Vector2 attackPosition;
    public int attack;

    [Header("Defense")]
    public bool showDefense;
    public Vector2 defensePosition;
    public int defense;

    [Header("Description")]
    public bool showDescription;
    public Vector2 descriptionPosition;
    public string descriptionText;

    [Header("Action Type")]
    public CardAction action;
}
