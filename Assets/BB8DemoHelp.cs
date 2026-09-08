using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BbRigidbodyController))]
public sealed class BB8DemoHelp : MonoBehaviour
{
    BbRigidbodyController controller;
    GUIStyle small, title, value;
    float displayedBattery;
    void Awake() { controller = GetComponent<BbRigidbodyController>(); displayedBattery = controller.Battery; }
    void Update()
    {
        displayedBattery = Mathf.MoveTowards(displayedBattery, controller.Battery, Time.deltaTime * 1.5f);
        if ((!FindFirstObjectByType<ConversationDirector>() && Input.GetKeyDown(KeyCode.R)) || transform.position.y < -8f)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    void OnGUI()
    {
        if(ExperienceShell.Instance)return;
        if (small == null)
        {
            small = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(0.72f, 0.79f, 0.79f) } };
            title = new GUIStyle(small) { fontSize = 15, fontStyle = FontStyle.Bold };
            value = new GUIStyle(title) { alignment = TextAnchor.UpperRight };
        }
        var oldMatrix = GUI.matrix;
        float scale = Mathf.Max(0.75f, Screen.height / 900f);
        GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
        float width = Screen.width / scale, height = Screen.height / scale;
        GUI.color = new Color(1, 1, 1, 0.85f);
        GUI.Label(new Rect(28, 25, 310, 24), "D E S G U A C E   / /   0 8", title);
        GUI.Label(new Rect(28, 50, 280, 20), "EXPLORACIÓN LIBRE", small);
        float y = height - 140;
        RectFill(new Rect(24, y, 278, 90), new Color(0.025f, 0.06f, 0.07f, 0.87f));
        string state = controller.IsBoosting ? "IMPULSO" : controller.Battery < 0.12f ? "BUSCA UNA CELDA" : "ENERGÍA";
        GUI.color = Color.white;
        GUI.Label(new Rect(40, y + 12, 190, 24), state, title);
        GUI.Label(new Rect(232, y + 12, 52, 24), Mathf.RoundToInt(displayedBattery * 100f) + "%", value);
        Color energy = controller.Battery < 0.12f ? new Color(1f, 0.58f, 0.25f) : new Color(0.3f, 0.88f, 0.94f);
        for (int i = 0; i < 10; i++)
        {
            RectFill(new Rect(40 + i * 24.5f, y + 42, 21, 5), new Color(0.23f, 0.3f, 0.31f));
            RectFill(new Rect(40 + i * 24.5f, y + 42, 21 * Mathf.Clamp01(displayedBattery * 10 - i), 5), energy);
        }
        GUI.color = Color.white;
        GUI.Label(new Rect(40, y + 59, 246, 22), "SHIFT · turbo   /   celdas azules · recarga", small);
        var controls = new GUIStyle(small) { alignment = TextAnchor.LowerRight };
        GUI.Label(new Rect(width - 670, height - 67, 642, 26), "WASD  mover    ·    ESPACIO  saltar    ·    MOUSE  mirar    ·    RUEDA  zoom", controls);
        GUI.color = Color.white;
        GUI.matrix = oldMatrix;
    }
    static void RectFill(Rect rect, Color color) { GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); }
}
