using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private int testNumber;
    public string NextScene;
    public TextMeshProUGUI debugTest;

    void Update()
    {
        debugTest.text = testNumber.ToString();
        testNumber++;
    }

    public void Load()
    {
        SceneManager.LoadSceneAsync(NextScene, LoadSceneMode.Single);
    }
}
