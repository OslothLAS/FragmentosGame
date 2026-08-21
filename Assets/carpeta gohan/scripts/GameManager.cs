using UnityEngine;

public class GameManager : MonoBehaviour
{
   public static GameManager instance;
   [Header("Número de monedas recolectadas")]
   public int monedas;
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void SumarMoneda()
    {
        monedas++;
    }
}
