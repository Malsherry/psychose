using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Méthode de lancement de la simulation
    public void StartBtn()
    {
        SceneManager.LoadScene("SimulationScene");
    }

    // Méthodes pour activer ou désactiver les éléments
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
