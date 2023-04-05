using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SWNetwork;
using System;

namespace UnityGinRummy
{
    [Serializable]
    public class GameDataEvent : UnityEvent<EncryptedData>
    {

    }

   public class NetCode : MonoBehaviour
    {
        public GameDataEvent OnGameDataReadyEvent = new GameDataEvent();
        public GameDataEvent OnGameDataChangedEvent = new GameDataEvent();
        public UnityEvent OnGameStateChangedEvent = new UnityEvent();

        RoomPropertyAgent roomPropertyAgent;
        RoomRemoteEventAgent roomRemoteEventAgent;

        const string ENCRYPTED_DATA = "EncryptedData";
        const string GAME_STATE_CHANGED = "GameStateChanged";

        public void ModifyGameData(EncryptedData encryptedData)
        {
            roomPropertyAgent.Modify(ENCRYPTED_DATA, encryptedData);
        }

        public void NotifyOtherPlayerGameStateChanged()
        {
            roomRemoteEventAgent.Invoke(GAME_STATE_CHANGED);
        }

        public void EnableRoomPropertyAgent()
        {
            roomPropertyAgent.Initialize();
        }

        public void OnEncryptedDataReady()
        {
            EncryptedData encryptedData = roomPropertyAgent.GetPropertyWithName(ENCRYPTED_DATA).GetValue<EncryptedData>();
            OnGameDataReadyEvent.Invoke(encryptedData);
        }

        public void OnEncryptedDataChanged()
        {
            EncryptedData encryptedData = roomPropertyAgent.GetPropertyWithName(ENCRYPTED_DATA).GetValue<EncryptedData>();
            OnGameDataChangedEvent.Invoke(encryptedData);
        }

        public void OnGameStateChangedRemoteEvent()
        {
            OnGameStateChangedEvent.Invoke();
        }

        private void Awake()
        {
            roomPropertyAgent = FindObjectOfType<RoomPropertyAgent>();
            roomRemoteEventAgent = FindObjectOfType<RoomRemoteEventAgent>();
        }
    }
}