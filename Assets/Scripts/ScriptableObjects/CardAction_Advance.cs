using UnityEngine;

[CreateAssetMenu(fileName = "ActionAdvance", menuName = "New Action/Create Action Advance")]
public class CardAction_Advance : CardAction
{
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

        //Move
        RectTransform cP = MainManager.TableCards.Grid[x, y].myCard.GetComponent<RectTransform>();
        if (cP != null)
        {
            MainManager.TableCards.MoveCardFromTo(cP, x, y, x, y + 1);
        }

        //Destroy Card
        RectTransform advCard = MainManager.HandCards.SelectedCard;
        MainManager.HandCards.RemoveCardFromHand(advCard);
        MainManager.TableCards.HighlightSlots(false);
        Destroy(advCard.gameObject);
    }

    private void HighlightPlayableSlots()
    {
        for (int x = 0; x < MainManager.TableCards.width; x++)
        {
            for(int y = 0; y < MainManager.TableCards.laneHeight-1; y++)
            {
                Slot_Base slotDown = MainManager.TableCards.Grid[x, y];
                Slot_Base slotUp = MainManager.TableCards.Grid[x, y+1];

                if (slotDown.myCard != null && slotDown.myCard.GetComponent<Card_Player>() != null && slotUp.myCard == null)
                {
                    slotDown.amClicked += PlayOnSlot;
                    slotDown.amHovered += ShowEffectGraphic;
                    slotDown.ToggleHighlight(true);
                }
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
