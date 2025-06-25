using Meta.WitAi;
using Meta.XR.ImmersiveDebugger.UserInterface;
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
        BabySpawner.spawnFootball = value;
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

    public void ToggleRedFilter(bool value)
    {
        ViewFilters.isActive = value;
    }

    public void ToggleCamera(bool value)
    {
        SpawnThings.spawnCamera = value;
    }

    public void ToggleSpiders(bool value)
    {
        SpiderSpawner.spawnSpiders = value;
    }

    public void ToggleDoorNoises(bool value)
    {
        SpawnThings.spawnDoorNoises = value;
    }

    public void ToggleMenuSpawn(bool value)
    {
        SpawnThings.spawnMenu = value;
    }
    public void ToggleRadioMusic(bool value)
    {
        Radio.ActiveRadio = value;
    }
    public void ToggleCafeAmbient(bool value)
    {
        Radio.ambianceCafe = value;
    }
    public void ToggleGlass(bool value)
    {
        GlassSpawner.glass = value;
    }
    public void ToggleSpots(bool value)
    { 
        SpawnThings.spawnWindowSpots = value;
    }
}
