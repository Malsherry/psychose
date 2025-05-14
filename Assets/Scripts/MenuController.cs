using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // M�thode de lancement de la simulation
    public void StartBtn()
    {
        SceneManager.LoadScene("SimulationScene");
    }

    // M�thodes pour activer ou d�sactiver les �l�ments
    public void ToggleSpawnPorteIFMS(bool value)
    {
        SpawnThings.spawnPorteIFMS = value;
    }

    public void ToggleSpawnWallDecoration(bool value)
    {
        SpawnThings.spawnWallDecoration = value;
    }

    public void ToggleSpawnFootball(bool value)
    {
        SpawnThings.spawnFootball = value;
    }

    public void ToggleSpawnBoardGames(bool value)
    {
        SpawnThings.spawnBoardGames = value;
    }

    public void ToggleSpawnWindowNoise(bool value)
    {
        SpawnThings.spawnWindowNoise = value;
    }

    public void ToggleSpawnCubes(bool value)
    {
        SpawnThings.spawnCubes = value;
    }
}
