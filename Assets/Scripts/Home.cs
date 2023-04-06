using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SWNetwork;

namespace UnityGinRummy
{
    public class Home : MonoBehaviour
    {
        public enum RoomState
        {
            Default,
            JoinedRoom
        }
        public RoomState State = RoomState.Default;

        public GameObject OnlinePopUp;
        public GameObject OnlineRoomPopUp;
        public GameObject Player1Position;
        public GameObject Player2Position;
        public GameObject Player1Icon;
        public GameObject Player2Icon;
        public GameObject StartButton;
        public Text WaitMessageText;
        public InputField UsernameInputField;

        string username;

        private void Start()
        {
            HideAllPopover();
            NetworkClient.Lobby.OnLobbyConnectedEvent += OnLobbyConnected;
            NetworkClient.Lobby.OnNewPlayerJoinRoomEvent += OnNewPlayerJoinRoomEvent;
            NetworkClient.Lobby.OnRoomReadyEvent += OnRoomReadyEvent;
        }

        void OnRoomReadyEvent(SWRoomReadyEventData eventData)
        {
            ConnectToRoom();
        }

        void ConnectToRoom()
        {
            NetworkClient.Instance.ConnectToRoom((connected) =>
            {
                if (connected)
                {
                    SceneManager.LoadScene("Multiplayer");
                }
                else
                {
                    Debug.Log("Failed connection");
                }
            });
        }

        void OnNewPlayerJoinRoomEvent(SWJoinRoomEventData eventData)
        {
            if (NetworkClient.Lobby.IsOwner)
            {
                ShowReadyToStart();
            }
        }

        private void OnDestroy()
        {
            if (NetworkClient.Lobby != null)
            {
                NetworkClient.Lobby.OnLobbyConnectedEvent -= OnLobbyConnected;
                NetworkClient.Lobby.OnNewPlayerJoinRoomEvent -= OnNewPlayerJoinRoomEvent;
            }
        }

        void ShowOnlinePopUp()
        {
            OnlinePopUp.SetActive(true);
        }

        void ShowOnlineRoomPopUp()
        {
            OnlinePopUp.SetActive(false);
            OnlineRoomPopUp.SetActive(true);
            Player1Position.SetActive(false);
            Player1Icon.SetActive(false);
            Player2Position.SetActive(false);
            Player2Icon.SetActive(false);
            StartButton.SetActive(false);
        }

        void ShowReadyToStart()
        {
            Player1Position.SetActive(true);
            Player1Icon.SetActive(true);
            Player2Position.SetActive(true);
            Player2Icon.SetActive(true);
            StartButton.SetActive(true);
            WaitMessageText.text = "";
        }

        void HideAllPopover()
        {
            OnlinePopUp.SetActive(false);
            OnlineRoomPopUp.SetActive(false);
            Player1Position.SetActive(false);
            Player1Icon.SetActive(false);
            Player2Position.SetActive(false);
            Player2Icon.SetActive(false);
            StartButton.SetActive(false);
            WaitMessageText.text = "";
        }

        void CheckInToRoom()
        {
            NetworkClient.Instance.CheckIn(username, (bool successful, string error) =>
            {
                if (!successful)
                {
                    Debug.LogError(error);
                }
            });
        }

        void RegisterToTheLobbyServer()
        {
            NetworkClient.Lobby.Register(username, (successful, reply, error) => {
                if (successful)
                {
                    Debug.Log("Lobby registered " + reply);
                    if (string.IsNullOrEmpty(reply.roomId))
                    {
                        JoinOrCreateRoom();
                    }
                    else if(reply.started)
                    {
                        State = RoomState.JoinedRoom;
                        ConnectToRoom();
                    }
                    else
                    {
                        State = RoomState.JoinedRoom;
                        ShowOnlineRoomPopUp();
                        GetPlayersInTheRoom();
                    }
                }
                else
                {
                    Debug.Log("Lobby failed to register " + reply);
                }
            });

        }

        void JoinOrCreateRoom()
        {
            NetworkClient.Lobby.JoinOrCreateRoom(false, 2, 60, (successful, reply, error) => {
                if (successful)
                {
                    Debug.Log("Joined or created room " + reply);
                    State = RoomState.JoinedRoom;
                    ShowOnlineRoomPopUp();
                    GetPlayersInTheRoom();
                }
                else
                {
                    Debug.Log("Failed to join or create room " + error);
                }
            });
        }

        void GetPlayersInTheRoom()
        {
            NetworkClient.Lobby.GetPlayersInRoom((successful, reply, error) => {
                if (successful)
                {
                    Debug.Log("Got players " + reply);
                    foreach (SWPlayer player in reply.players)
                        Debug.Log("Player custom data: " + player.GetCustomDataString());
                        
                    if(reply.players.Count == 1)
                    {
                        Player1Position.SetActive(true);
                        Player1Icon.SetActive(true);
                        WaitMessageText.text = "Waiting on another player...";
                    }
                    else
                    {
                        Player1Position.SetActive(true);
                        Player1Icon.SetActive(true);
                        Player2Position.SetActive(true);
                        Player2Icon.SetActive(true);
                        WaitMessageText.text = "";

                        if (NetworkClient.Lobby.IsOwner)
                        {
                            ShowReadyToStart();
                        }
                    }
                }
                else
                {
                    Debug.Log("Failed to get players " + error);
                }
            });
        }

        void StartRoom()
        {
            NetworkClient.Lobby.StartRoom((successful, error) => {
                if (successful)
                {
                    Debug.Log("Started room.");
                }
                else
                {
                    Debug.Log("Failed to start room " + error);
                }
            });
        }

        void LeaveRoom()
        {
            NetworkClient.Lobby.LeaveRoom((successful, error) => {
                if (successful)
                {
                    Debug.Log("Left room");
                    State = RoomState.Default;
                }
                else
                {
                    Debug.Log("Failed to leave room " + error);
                }
            });
        }

        void OnLobbyConnected()
        {
            RegisterToTheLobbyServer();
        }

        public void OnOnlineClicked()
        {
            ShowOnlinePopUp();
        }

        public void OnUsernameOkayClicked()
        {
            username = UsernameInputField.text;

            Debug.Log("OnUsernameOkayClicked " + username);

            ShowOnlineRoomPopUp();
            CheckInToRoom();
        }

        public void OnUsernameCancelClicked()
        {
            Debug.Log("Cancelled");
            if (State == RoomState.JoinedRoom)
            {
                LeaveRoom();
            }

            HideAllPopover();
        }

        public void OnOnlineRoomStartClicked()
        {
            StartRoom();
        }

        public void playComputer()
        {
            SceneManager.LoadScene("Game");
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