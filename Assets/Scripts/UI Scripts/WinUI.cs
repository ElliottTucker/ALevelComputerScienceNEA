using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinUI : MonoBehaviour
{
    public TMP_Text toMenuText;
    public GameObject toMenuButton;
    private SaveData saveData;

    public void ReachEnd()
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        toMenuButton.SetActive(true);
    }

    public void GoToMenu()
    {
        SaveSystem.DeleteData();
        SceneManager.LoadScene(0);
    }

    private void Start()
    {
        saveData = FindFirstObjectByType<SaveData>();
        toMenuText.text = "Return to menu";
        toMenuButton.SetActive(false);
    }
}
