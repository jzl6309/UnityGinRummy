using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SWNetwork;
using System;

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

        [Serializable]
        public class RoomData
        {
            public string name;
        }

        public RoomData roomData;

        public GameObject OnlinePopUp;
        public GameObject OnlineRoomPopUp;
        public GameObject Player1Position;
        public GameObject Player2Position;
        public GameObject Player1Icon;
        public Text Player1Name;
        public Text Player2Name;
        public GameObject Player2Icon;
        public GameObject StartButton;
        public Text WaitMessageText;
        public InputField UsernameInputField;
        public GameObject OnlineStep2;
        public GameObject CreateRoom;
        public InputField gameRoomName;
        public GameObject roomList;

        public GameObject scrollViewContent;

        public GameObject buttonTemplate;

        string username;

        private void Start()
        {
            HideAllPopover();
            NetworkClient.Lobby.OnLobbyConnectedEvent += OnLobbyConnected;
            NetworkClient.Lobby.OnNewPlayerJoinRoomEvent += OnNewPlayerJoinRoomEvent;
            NetworkClient.Lobby.OnRoomReadyEvent += OnRoomReadyEvent;
            NetworkClient.Lobby.OnPlayerLeaveRoomEvent += PlayerLeftEvent;
        }
        
        void PlayerLeftEvent(SWLeaveRoomEventData eventData)
        {
            GetPlayersInTheRoom();
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
                NetworkClient.Lobby.OnRoomReadyEvent -= OnRoomReadyEvent;
                NetworkClient.Lobby.OnPlayerLeaveRoomEvent -= PlayerLeftEvent;
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
            CreateRoom.SetActive(false);
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
            OnlineStep2.SetActive(false);
            CreateRoom.SetActive(false);
            roomList.SetActive(false);
            WaitMessageText.text = "";
        }

        void StateChangeCreateRoom()
        {
            OnlineStep2.SetActive(false);
            CreateRoom.SetActive(true);
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

        public void CreateNewRoom()
        {
            roomData = new RoomData();
            roomData.name = gameRoomName.text;

            // use the serializable roomData_ object as room's custom data.
            NetworkClient.Lobby.CreateRoom(roomData, true, 2, (successful, reply, error) =>
            {
                if (successful)
                {
                    Debug.Log("Room created " + reply);

                    // refresh the room list
                    //GetRooms();

                    // refresh the player list
                    //GetPlayersInCurrentRoom();
                    State = RoomState.JoinedRoom;
                    ShowOnlineRoomPopUp();
                    GetPlayersInTheRoom();
                }
                else
                {
                    Debug.Log("Failed to create room " + error);
                }
            });
        }

        public void RegisterPlayer()
        {
            username = UsernameInputField.text;
            NetworkClient.Instance.CheckIn(username, (bool successful, string error) =>
            {
                if (!successful)
                {
                    Debug.LogError(error);
                } else
                {
                    OnlinePopUp.SetActive(false);
                    OnlineStep2.SetActive(true);
                    Debug.Log("Registered " + username);
                }
            });
        }

        private void OnRoomSelected(string roomId)
        {
            Debug.Log("OnRoomSelected: " + roomId);
            // Join the selected room
            NetworkClient.Lobby.JoinRoom(roomId, (successful, reply, error) =>
            {
                if (successful)
                {
                    Debug.Log("Joined room " + reply);
                    State = RoomState.JoinedRoom;
                    roomList.SetActive(false);
                    ShowOnlineRoomPopUp();
                    GetPlayersInTheRoom();
                }
                else
                {
                    Debug.Log("Failed to Join room " + error);
                }
            });
        }

        public void GetRooms()
        {
            // Get the rooms for the current page.
            NetworkClient.Lobby.GetRooms(0, 15, (successful, reply, error) =>
            {
                if (successful)
                {
                    Debug.Log("Got rooms " + reply);

                    OnlineStep2.SetActive(false);
                    roomList.SetActive(true);

                    foreach (SWRoom room in reply.rooms)
                    {
                        Debug.Log(room);
                        RoomData rData = room.GetCustomData<RoomData>();
                        GameObject btn = Instantiate(buttonTemplate);
                        btn.GetComponentInChildren<Text>().text = rData.name;
                        btn.transform.SetParent(scrollViewContent.transform);
                        btn.GetComponent<Button>().onClick.AddListener(delegate { OnRoomSelected(room.id); });

                    }
                }
                else
                {
                    Debug.Log("Failed to get rooms " + error);
                }
            });
        }

        void RegisterToTheLobbyServer()
        {
            Debug.Log("Lobby Connect Event");
            NetworkClient.Lobby.Register(username, (successful, reply, error) => {
                if (successful)
                {
                    Debug.Log("Lobby registered " + reply);
                    if (reply.started)
                    {
                        // Player is in a room and the room has started.
                        // Call NetworkClient.Instance.ConnectToRoom to connect to the game servers of the room.
                    }
                    else if (reply.roomId != null)
                    {
                        // Player is in a room.
                        //GetRooms();
                        //GetPlayersInCurrentRoom();
                    }
                    else
                    {
                        // Player is not in a room.
                        //GetRooms();
                    }
                }
                else
                {
                    Debug.Log("Lobby failed to register " + error);
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
                        Player1Name.text = reply.players[0].data;
                        Player2Position.SetActive(false);
                        Player2Icon.SetActive(false);
                        Player2Name.text = "";
                        WaitMessageText.text = "Waiting on another player...";
                    }
                    else
                    {
                        Player1Position.SetActive(true);
                        Player1Icon.SetActive(true);
                        Player1Name.text = reply.players[0].data;
                        Player2Position.SetActive(true);
                        Player2Icon.SetActive(true);
                        Player2Name.text = reply.players[1].data;
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

        public void showLobbiesClicked()
        {
            username = UsernameInputField.text;

            Debug.Log("showLobbiesClicked " + username);

            CheckInToRoom();
        }

        public void OnUsernameCancelClicked()
        {
            Debug.Log("Cancelled");
            if (State == RoomState.JoinedRoom)
            {
                State = RoomState.Default;
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