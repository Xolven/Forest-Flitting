using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadAScene : MonoBehaviour
{
    [SerializeField] public string targetSceneName;
    public void LoadScene()
    {
        //Debug.Log("loading scene: " + sceneName);
        SceneManager.LoadScene(targetSceneName);
    }
}
