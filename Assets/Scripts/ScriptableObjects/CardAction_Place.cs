using UnityEngine;

[CreateAssetMenu(fileName = "ActionPlace", menuName = "New Action/Create Action Place")]
public class CardAction_Place : CardAction
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
        MainManager.Audio.PlaySound(ActionSound, 0.5f);
        MainManager.TableCards.SetEffectImages(false);

        RectTransform sC = MainManager.HandCards.SelectedCard;
        MainManager.TableCards.PlaceCardInSlot(sC, x, y);
        MainManager.HandCards.RemoveCardFromHand(sC);
        MainManager.TableCards.HighlightSlots(false);
    }

    private void HighlightPlayableSlots()
    {
        for (int i = 0; i < MainManager.TableCards.width; i++)
        {
            if (MainManager.TableCards.Grid[i, 0].myCard == null)
            {
                MainManager.TableCards.Grid[i, 0].amClicked += PlayOnSlot;
                MainManager.TableCards.Grid[i, 0].amHovered += ShowEffectGraphic;
                MainManager.TableCards.Grid[i, 0].ToggleHighlight(true);
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
