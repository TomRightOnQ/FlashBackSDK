using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// FlackBackSDK MANAGER
/// Manager the loading and life cycle of scenes and levels
/// </summary>
public class FBLevelSystem : FBGameSystem
{
    public override void OnSystemInit()
    {

    }

    // DO NOT USE
    public override void OnSceneChange()
    {

    }

    public override void ManualInit()
    {

    }

    // Public:
    /// <summary>
    /// Load the scene by scene name
    /// </summary>
    /// <param name="sceneName"> Scene name </param>
    public void LoadScene(string sceneName)
    {
        // 开始加载场景
        FBMainGame.Game.EventSystem.PostEvent(GameEvent.Event.SCENE_LOAD_BEGIN);
        StartCoroutine(loadSceneAsync(sceneName));
    }

    // Private
    private IEnumerator loadSceneAsync(string sceneName)
    {
        // Start loading the scene
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        // Loading procedure
        // Notify the game loop when the scene is loaded
        FBMainGame.Game.OnSceneLoaded();
        
        // Complete
        sceneLoaded();
    }

    private void sceneLoaded() 
    {
        FBMainGame.Game.EventSystem.PostEvent(GameEvent.Event.SCENE_LOAD_COMPLETE);
    }
}
