using UnityEngine;

[CreateAssetMenu(fileName = "ActionFuse", menuName = "New Action/Create Action Fuse")]
public class CardAction_Fuse : CardAction
{
    public override void CardSelected()
    {
        //Play Sound
        MainManager.Audio.PlaySound(ActionSound, 0.5f);

        RectTransform sC = MainManager.HandCards.SelectedCard;
        MainManager.Turn.AddFuse(sC);
        MainManager.HandCards.RemoveCardFromHand(sC);
    }

    public override void CardDeselected()
    {
        //
    }
}
