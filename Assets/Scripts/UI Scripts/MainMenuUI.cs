using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public TMP_Text loadSaveText;
    public TMP_Text newSaveText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SaveData.instance != null)
        {
            float heightPercent = SaveData.instance.GetHeightPercent();
            if (heightPercent > 0)
            {
                loadSaveText.text = "Load current saved game " + heightPercent.ToString("F1")+"%";
            }
            else
            {
                loadSaveText.text = "Load current saved game";
            }
        }
        else
        {
            loadSaveText.text = "Load current saved game";
        }   
        newSaveText.text = "Create new game";
    }

    public void LoadSave()
    {
        SceneManager.LoadScene(1);
    }

    public void NewGame()
    {
        SaveSystem.DeleteData();
        if (SaveData.instance != null)
        {
            SaveData.instance.ResaveData(); // reset in-memory save
        }

        SceneManager.LoadScene(1);
    }
    
}
