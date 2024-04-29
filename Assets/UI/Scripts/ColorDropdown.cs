using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorDropdown : MonoBehaviour
{

    public void OnChange()
    {

        if(gameObject.GetComponent<TMP_Dropdown>().value == 0)
        {
            Config.PlayerColor = SpotState.WHITE;
        }
        else
        {
            Config.PlayerColor = SpotState.BLACK;

        }
        Debug.Log(Config.PlayerColor);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
