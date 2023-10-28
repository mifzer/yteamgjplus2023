using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
namespace GJPlus2023
{
    public enum ElementStatus { Basic, Fire, Thunder, Water }
    public class _GameManager : MonoBehaviour
    {
        public enum GameStatus { MainMenu, Pause, Play, Gameover }

        [FoldoutGroup("Manager")] public static GameStatus StatusGame;
        [FoldoutGroup("Manager")][SerializeField] private _PlayerController _playerController;
        [FoldoutGroup("Manager")][SerializeField] private  ChatManager chatManager;
        [FoldoutGroup("Manager")][SerializeField] private PlayerInput playerInput;

        #region  Main Menu
        [FoldoutGroup("Main Menu")][SerializeField] private GameObject _PanelMainMenu;

        private void Start()
        {
            _playerController.LockCursor(false);
            _playerController.enabled = false;
            playerInput.enabled = false;
            GetDataSoundPlayerPref();
            StatusGame = GameStatus.MainMenu;
            _PanelMainMenu.SetActive(true);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (StatusGame != GameStatus.Play) return;
                TogglePause();
            }
        }
        public void ButtonStartGame()
        {
            _PanelMainMenu.SetActive(false);
            _playerController.LockCursor(true);
            _playerController.enabled = true;
            playerInput.enabled = true;
            StatusGame = GameStatus.Play;
            chatManager.OpenConversation(0);
        }
        public void ButtonContinuePlay()
        {
            // Do Continue Play
        }
        public void ButtonBackToMainMenu()
        {
            SceneManager.LoadScene(0);
        }
        public void ButtonExitGame()
        {
            Application.Quit();
        }
        #endregion

        #region Pause Game
        [FoldoutGroup("Pause Game")][SerializeField] private GameObject _PanelPause;
        [FoldoutGroup("Pause Game")] float previousTimeScale = 1;
        public void TogglePause()
        {
            if (Time.timeScale > 0)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0;
                StatusGame = GameStatus.Pause;
                _PanelPause.SetActive(true);
                _playerController.LockCursor(false);
                _playerController.enabled = false;
                playerInput.enabled = false;
            }
            else if (Time.timeScale == 0)
            {
                Time.timeScale = previousTimeScale;
                StatusGame = GameStatus.Play;
                _PanelPause.SetActive(false);
                _playerController.LockCursor(true);
                _playerController.enabled = true;
                playerInput.enabled = true;
            }
        }
        #endregion

        #region Game Over
        [FoldoutGroup("Game OVer")] public GameObject _PanelGameOver;
        public void GameOver()
        {
            StatusGame = GameStatus.Gameover;
            _PanelGameOver.SetActive(true);
        }
        #endregion

        #region Option Menu
        [FoldoutGroup("Option Menu")][SerializeField] private GameObject _panelOption;
        [FoldoutGroup("Option Menu")][SerializeField] private AudioMixer _audioMixer;
        [FoldoutGroup("Option Menu")][SerializeField] private Toggle _toggleSoundYes, _toggleSoundNo;
        [FoldoutGroup("Option Menu")] private bool _isMute;
        [FoldoutGroup("Option Menu")][SerializeField] private Slider _SliderBgmVol, _SliderSfxVol;
        [FoldoutGroup("Option Menu")] private float _BgmVolume, _SfxVolume;
        [FoldoutGroup("Option Menu")] private const string PREF_MUTE = "GameMute", PREF_BGM_VOLUME = "BgmVolume", PREF_SFX_VOLUME = "SfxVolume", MASTER_MIXER = "MasterMixer", BGM_MIXER = "BgmMixer", SFX_MIXER = "SfxMixer";

        void GetDataSoundPlayerPref()
        {
            _isMute = PlayerPrefs.GetInt(PREF_MUTE, 0) == 1;
            _BgmVolume = PlayerPrefs.GetFloat(PREF_BGM_VOLUME, 20f);
            _SfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, 30f);
            SetToggleSoundYes(!_isMute);
            SetToggleSoundNo(_isMute);
            SetSBarBgm(_BgmVolume);
            SetSBarSfx(_SfxVolume);
        }
        public void OpenCloseOptionMenu(bool value) => _panelOption.SetActive(value);
        public void SetToggleSoundYes(bool value)
        {
            if (!value) return;
            Debug.Log("Togle Sound Yes");
            _toggleSoundYes.isOn = value;
            _isMute = !value;
            _audioMixer.SetFloat(MASTER_MIXER, _isMute ? 0f : 20f);
            PlayerPrefs.SetInt(PREF_MUTE, _isMute ? 1 : 0);
        }
        public void SetToggleSoundNo(bool value)
        {
            if (!value) return;
            _toggleSoundNo.isOn = value;
            _isMute = value;
            _audioMixer.SetFloat(MASTER_MIXER, _isMute ? 0f : 20f);
            PlayerPrefs.SetInt(PREF_MUTE, _isMute ? 1 : 0);
        }
        public void SetSBarBgm(float value)
        {
            _BgmVolume = value;
            _audioMixer.SetFloat(BGM_MIXER, _BgmVolume);
            PlayerPrefs.SetFloat(PREF_BGM_VOLUME, (Mathf.Log10(_BgmVolume / 100) * 20) - 10);
        }
        public void SetSBarSfx(float value)
        {
            _SfxVolume = value;
            _audioMixer.SetFloat(SFX_MIXER, _SfxVolume);
            PlayerPrefs.SetFloat(PREF_SFX_VOLUME, (Mathf.Log10(_SfxVolume / 100) * 20) - 10);
        }
        #endregion
    }
}