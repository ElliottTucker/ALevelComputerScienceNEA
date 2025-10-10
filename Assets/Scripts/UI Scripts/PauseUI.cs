using TMPro;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PauseUI : MonoBehaviour
{
    public TMP_Text resumeText;
    public GameObject resumeButton;
    public TMP_Text toMenuText;
    public GameObject toMenuButton;
    private SaveData saveData;
    
    public void ResumeGame()
    {
        Time.timeScale = 1;
        resumeButton.SetActive(false);
        toMenuButton.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void PauseGame()
    {
        Time.timeScale = 0;
        resumeButton.SetActive(true);  
        toMenuButton.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ToMainMenu()
    {
        saveData.SaveCurrentGame();
       
        SceneManager.LoadScene(0);
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveData = GameObject.FindFirstObjectByType<SaveData>();
        resumeText.text = "click to resume";
        toMenuText.text = "Save and exit";
        resumeButton.SetActive(false);
        toMenuButton.SetActive(false);
    }

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0)
            {
                ResumeGame();
            }
            else 
            {
                PauseGame();
            }
        }
        
    }
}
