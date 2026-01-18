using UnityEngine;

[CreateAssetMenu(fileName = "ActionHeal", menuName = "New Action/Create Action Heal")]
public class CardAction_Heal : CardAction
{
    public int healAmount;

    [Header("Effect Images")]
    public Sprite effectPlace;
    public Vector2 effectPlaceOff;

    public override void CardSelected()
    {
        HighlightPlayableSlots();
    }

    public override void CardDeselected()
    {
        MainManager.TableCards.HighlightSlots(false);
        MainManager.TableCards.SetEffectImages(false);
    }

    private void PlayOnSlot(int x, int y)
    {
        //Play Sound
        MainManager.Audio.PlaySound(ActionSound);
        MainManager.TableCards.SetEffectImages(false);

        Card_Player cP = MainManager.TableCards.Grid[x, y].myCard.GetComponent<Card_Player>();
        if(cP != null )
        {
            cP.Heal(healAmount);
        }

        //Destroy Card
        RectTransform healCard = MainManager.HandCards.SelectedCard;
        MainManager.HandCards.RemoveCardFromHand(healCard);
        MainManager.TableCards.HighlightSlots(false);
        Destroy(healCard.gameObject);
    }

    private void HighlightPlayableSlots()
    {
        foreach (Slot_Base slot in MainManager.TableCards.Grid)
        {
            if(slot.myCard != null && slot.myCard.GetComponent<Card_Player>() != null)
            {
                slot.amClicked += PlayOnSlot;
                slot.amHovered += ShowEffectGraphic;
                slot.ToggleHighlight(true);
            }
        }
    }

    private void ShowEffectGraphic(int x, int y, bool state)
    {
        if (state)
            MainManager.TableCards.SetEffectImages(effectPlace, 0, MainManager.TableCards.Grid[x, y].position, effectPlaceOff);
        else
            MainManager.TableCards.SetEffectImages(false);
    }
}
