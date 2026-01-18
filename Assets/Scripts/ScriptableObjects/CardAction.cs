using UnityEngine;

public class CardAction : ScriptableObject
{
    public AudioClip ActionSound;

    public virtual void CardSelected()
    {
        //
    }

    public virtual void CardDeselected()
    {
        //
    }
}
