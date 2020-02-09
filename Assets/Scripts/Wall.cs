using UnityEngine;

public class Wall : MonoBehaviour
{
    // Start is called before the first frame update
    public Sprite dmgSprite;                    //Alternate sprite to display after Wall has been attacked by player.
    public int hp = 3;                            //hit points for the wall.
    public AudioClip chopSound1;
    public AudioClip chopSound2;

    private SpriteRenderer spriteRenderer;        //Store a component reference to the attached SpriteRenderer.


    void Awake ()
    {
        //Get a component reference to the SpriteRenderer.
        spriteRenderer = GetComponent<SpriteRenderer> ();
    }


    //DamageWall is called when the player attacks a wall.
    public void DamageWall (int loss)
    {
        SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);
        //Set spriteRenderer to the damaged wall sprite.
        spriteRenderer.sprite = dmgSprite;

        //Subtract loss from hit point total.
        hp -= loss;

        //If hit points are less than or equal to zero:
        if(hp <= 0)
            //Disable the gameObject.
            gameObject.SetActive (false);
    }
}
