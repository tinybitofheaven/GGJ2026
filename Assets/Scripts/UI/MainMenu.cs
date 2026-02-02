using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    
    public GameObject hostMenu;
    public GameObject joinMenu;
    public GameObject lobbyMenu;

    public void OpenHostGameMenu()
    {
        DisableAllMenus();
        
        hostMenu.SetActive(true);
    }
    
    public void OpenJoinGameMenu()
    {
        DisableAllMenus();

        joinMenu.SetActive(true);
    }
    
    public void Quit()
    {
        Application.Quit();
    }

    public void BackToMainMenu()
    {
        DisableAllMenus();
        
        mainMenu.SetActive(true);
    }

    public void OpenLobbyMenu()
    {
        DisableAllMenus();

        lobbyMenu.SetActive(true);
    }

    public void DisableAllMenus()
    {
        mainMenu.SetActive(false);
        hostMenu.SetActive(false);
        joinMenu.SetActive(false);
        lobbyMenu.SetActive(false);
    }
}
