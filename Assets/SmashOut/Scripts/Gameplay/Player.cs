using UnityEngine;

public class Player : MonoBehaviour
{
    //trigger game over if ball hit by smasher
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") && collision.gameObject.transform.position.y > transform.position.y)
        {
            GameManager.Instance.GameOver();
        }
    }
}
