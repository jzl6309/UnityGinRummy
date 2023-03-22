using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace UnityGinRummy
{
    public class Home : MonoBehaviour
    {
        public enum LobbyState
        {
            Default,
            JoinedRoom
        }

        public GameObject OnlinePopUp;
        public InputField UsernameInputField;

        string username; 

        private void Start()
        {
            HideAllPopover();
        }

        void ShowOnlinePopUp()
        {
            OnlinePopUp.SetActive(true);
        }

        void HideAllPopover()
        {
            OnlinePopUp.SetActive(false);
        }

        public void OnOnlineClicked()
        {
            ShowOnlinePopUp();
        }

        public void OnUsernameOkayClicked()
        {
            username = UsernameInputField.text;

            Debug.Log("OnUsernameOkayClicked " + username);
        }

        public void OnUsernameCancelClicked()
        {
            Debug.Log("Cancelled");
            HideAllPopover();
        }


        public void playComputer()
        {
            SceneManager.LoadScene(0);
        }

        public void quit()
        {
            #if UNITY_STANDALONE
                    Application.Quit();
            #endif
            #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}