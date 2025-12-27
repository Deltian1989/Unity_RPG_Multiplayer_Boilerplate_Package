using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MidniteOilSoftware.Multiplayer
{
    public class CharacterManager : SingletonNetworkBehavior<CharacterManager>
    {
        [SerializeField] NetworkPlayer playerPrefab;

        [SerializeField] string playerSpawnPointTagName = "PlayerSpawnPoint";

        public NetworkList<CharacterData> CharacterData = new(null,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public CharacterData LocalCharacterData;

        public event Action<NetworkListEvent<CharacterData>> OnPlayerCharacterSelectionChanged;

        protected override void Awake()
        {
            base.Awake();

            CharacterData.OnListChanged += OnCharacterDataListChanged;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleLoadEventCompletedForAllPlayers;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleLoadEventCompletedForAllPlayers;
        }

        public CharacterData GetCharacterData(ulong clientId)
        {
            for (int i = 0; i < CharacterData.Count; i++)
            {
                var characterDataEntry = CharacterData[i];
                if (characterDataEntry.clientId == clientId)
                {
                    return characterDataEntry;
                }
            }
            if (_enableDebugLog)
            {
                Debug.LogError($"CharacterManager:Multiplayer-GetCharacterData - No character data found for ClientId: {clientId}");
            }
            return new CharacterData();
        }

        public void AddNewCharacterData(ulong clientId)
        {
            var newCharacterData = new CharacterData
            {
                clientId = clientId
            };

            CharacterData.Add(newCharacterData);
        }

        public void SetCharacterSelection(ulong clientId, int characterId, string characterName)
        {
            for (int i = 0; i < CharacterData.Count; i++)
            {
                var characterDataEntry = CharacterData[i];

                if (characterDataEntry.clientId == clientId)
                {
                    characterDataEntry.SetCharacterIdAndName(characterId, characterName);
                    CharacterData[i] = characterDataEntry;
                    return;
                }
            }

            if (_enableDebugLog)
            {
                Debug.LogError($"CharacterManager:Multiplayer-SetCharacterSelection - No character data found for ClientId: {clientId}");
            }

        }

        public virtual bool CheckIsCurrentSceneLevelScene(string sceneName)
        {
            return false;
        }

        private void OnCharacterDataListChanged(NetworkListEvent<CharacterData> changeEvent)
        {
            if (changeEvent.Type == NetworkListEvent<CharacterData>.EventType.Add &&
                changeEvent.Value.clientId == NetworkManager.Singleton.LocalClientId)
            {
                LocalCharacterData = changeEvent.Value;
            }
            else if (changeEvent.Type == NetworkListEvent<CharacterData>.EventType.Remove &&
                     changeEvent.Value.clientId == NetworkManager.Singleton.LocalClientId)
            {
                LocalCharacterData = new();
            }

            OnPlayerCharacterSelectionChanged?.Invoke(changeEvent);
        }

        void HandleLoadEventCompletedForAllPlayers(string sceneName, LoadSceneMode loadSceneMode,
                                                     List<ulong> clientsCompleted,
                                                     List<ulong> clientsTimedOut)
        {
            if (CheckIsCurrentSceneLevelScene(sceneName))
            {
                if (_enableDebugLog)
                {
                    Debug.Log($"Multiplayer:ProjectSceneManager - HandleLoadEventCompletedForAllPlayers {sceneName} {loadSceneMode} Completed:{clientsCompleted.Count} TimedOut:{clientsTimedOut.Count}");
                }

                for (int i = 0; i < CharacterData.Count; i++)
                {
                    ulong clientId = clientsCompleted[i];

                    if (_enableDebugLog)
                    {
                        Debug.Log($"Multiplayer:ProjectSceneManager - Client {clientId} completed loading scene {sceneName}");
                    }

                    var spawnPoints = GameObject.FindGameObjectsWithTag(playerSpawnPointTagName);

                    var player = Instantiate(playerPrefab, spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);

                    player.InitializePlayerSpawn();

                    if (_enableDebugLog)
                    {
                        Debug.Log($"Multiplayer:ProjectSceneManager - Spawning player for ClientId {clientId} at SpawnPoint x: {spawnPoints[i].transform.position.x}, y: {spawnPoints[i].transform.position.y}, z: {spawnPoints[i].transform.position.z}");
                    }
                }
            }
        }
    }
}
