using System.Collections;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cinematic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(CambiarEscena());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator CambiarEscena()
    {
        yield return new WaitForSeconds(2.1f);
        SceneManager.LoadScene("FrancoScene");
    }
}
