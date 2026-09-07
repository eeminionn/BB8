using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class BB8DemoHelp : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
        GUI.Box(new Rect(12, 12, 560, 64), "WASD: mover   |   Mouse: mirar   |   Espacio: saltar\nRueda: zoom   |   R: reiniciar", style);
    }
}
