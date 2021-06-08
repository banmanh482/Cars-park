using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject CreditUI;

    public void exitgame()
    {
        Application.Quit();
    }

    public void openCredit()
    {
        CreditUI.SetActive(true);
    }

    public void closeCredit()
    {
        CreditUI.SetActive(false);
    }

    public void startgame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
