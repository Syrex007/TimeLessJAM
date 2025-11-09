using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{

    [SerializeField] private Animator anim;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(SceneManager.GetActiveScene().buildIndex == 2)
        {
            StartCoroutine(ChangeSceneFromCinematic("FrancoScene"));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        StartCoroutine(SceneLoad("LetterScene"));
    }

    public void GoToGameplay()
    {
        StartCoroutine(SceneLoad("Mansion"));
    }

       public void GoToMain()
    {
        StartCoroutine(SceneLoad("Main Menu"));
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void Credits()
    {
        
        StartCoroutine (SceneLoad("Credits"));
    }

    public IEnumerator SceneLoad(string sceneIndex)
    {
        //Triggerear animaci�n de fade
        anim.SetTrigger("StartTrans");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(sceneIndex);

    }

    public IEnumerator ChangeSceneFromCinematic(string sceneName)
    {
        yield return new WaitForSeconds(2.1f);
        SceneManager.LoadScene(sceneName);
    }
}
