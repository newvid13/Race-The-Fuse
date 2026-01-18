using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "New Level/Create Level")]
public class Level : ScriptableObject
{
    public string levelName;
    public string levelDifficulty;
    public string levelDescription;

    public string sceneName;
}
