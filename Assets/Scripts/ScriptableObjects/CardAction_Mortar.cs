using UnityEngine;

[CreateAssetMenu(fileName = "ActionMortar", menuName = "New Action/Create Action Mortar")]
public class CardAction_Mortar : CardAction
{
    public int damage;

    [Header("Effect Images")]
    public Sprite effectDamage;
    public Vector2 effectDamageOff;
    public Sprite effectLeft;
    public Vector2 effectLeftOff;
    public Sprite effectRight;
    public Vector2 effectRightOff;

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

        //Do damage
        GameObject targetedCard = MainManager.TableCards.Grid[x, y].myCard;
        if (targetedCard)
        {
            Card_Base crd = targetedCard.GetComponent<Card_Base>();

            crd.Damage(damage);
        }

        //push left
        if(x > 0)
        {
            PushSurroundingCard(x - 1, y, -1);
        }

        //push right
        if(x < 2)
        {
            PushSurroundingCard(x + 1, y, +1);
        }

        //Destroy Card
        RectTransform mortarCard = MainManager.HandCards.SelectedCard;
        MainManager.HandCards.RemoveCardFromHand(mortarCard);
        MainManager.TableCards.HighlightSlots(false);
        Destroy(mortarCard.gameObject);
    }

    private async void PushSurroundingCard(int x, int y, int direction)
    {
        if (MainManager.TableCards.Grid[x, y].myCard == null)
            return;

        await Awaitable.WaitForSecondsAsync(0.4f);

        RectTransform cardToMove = MainManager.TableCards.Grid[x, y].myCard.GetComponent<RectTransform>();

        if (x + direction >= 0 && x + direction < MainManager.TableCards.width)
        {
            MainManager.TableCards.MoveCardFromTo(cardToMove, x, y, x + direction, y);
        }
    }

    private void HighlightPlayableSlots()
    {
        foreach (Slot_Base slot in MainManager.TableCards.Grid)
        {
            slot.amClicked += PlayOnSlot;
            slot.amHovered += ShowEffectGraphic;
            slot.ToggleHighlight(true);
        }
    }

    private void ShowEffectGraphic(int x, int y, bool state)
    {
        if (state)
        {
            MainManager.TableCards.SetEffectImages(effectDamage, 0, MainManager.TableCards.Grid[x, y].position, effectDamageOff);

            if (x - 1 >= 0 && MainManager.TableCards.Grid[x - 1, y].myCard != null)
            {
                MainManager.TableCards.SetEffectImages(effectLeft, 1, MainManager.TableCards.Grid[x - 1, y].position, effectLeftOff);
            }

            if (x + 1 < MainManager.TableCards.width && MainManager.TableCards.Grid[x + 1, y].myCard != null)
            {
                MainManager.TableCards.SetEffectImages(effectRight, 2, MainManager.TableCards.Grid[x + 1, y].position, effectRightOff);
            }
        }
        else
        {
            MainManager.TableCards.SetEffectImages(false);
        }
    }
}
