using UnityEngine;
using UnityEngine.SceneManagement;

namespace HorrorGame.Core
{
    /// <summary>Mengatur state game (Playing / Paused / Won / GameOver).</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Playing;

        public static bool IsPlaying => Instance == null || Instance.State == GameState.Playing;

        private void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
        }

        private void Start()
        {
            SetState(GameState.Playing);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (State == GameState.Playing) SetState(GameState.Paused);
                else if (State == GameState.Paused) SetState(GameState.Playing);
            }
        }

        public void WinGame()
        {
            SetState(GameState.Won);
        }

        public void LoseGame()
        {
            SetState(GameState.GameOver);
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Quit()
        {
            Application.Quit();
        }

        private void SetState(GameState newState)
        {
            State = newState;
            bool playing = newState == GameState.Playing;

            Time.timeScale = playing ? 1f : 0f;
            Cursor.lockState = playing ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !playing;

            GameEvents.StateChanged.Raise(newState);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
