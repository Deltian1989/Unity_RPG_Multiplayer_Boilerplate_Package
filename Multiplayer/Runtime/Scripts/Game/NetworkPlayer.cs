using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.Authentication;
using System;

namespace MidniteOilSoftware.Multiplayer
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] bool _enableDebugLog = true;

        public ulong ConnectionId => OwnerClientId;
        public NetworkVariable<FixedString32Bytes> PlayerId;
        public NetworkVariable<FixedString32Bytes> PlayerName;
        public NetworkVariable<int> CharacterId;
        public NetworkVariable<FixedString32Bytes> CharacterName;

        GameManager _gameManager;

        GameManager GameManager
        {
            get
            {
                if (!_gameManager) _gameManager = FindFirstObjectByType<GameManager>();
                return _gameManager;
            }
        }

        void Awake()
        {
            PlayerId = new();
            PlayerName = new();
            DontDestroyOnLoad(gameObject);
            
            if (_enableDebugLog)
                Debug.Log("NetworkPlayer:Multiplayer-NetworkPlayer Awake - DontDestroyOnLoad set");
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
    
            if (_enableDebugLog)
                Debug.Log($"NetworkPlayer:Multiplayer-NetworkPlayer OnNetworkSpawn - ClientId: {OwnerClientId}, IsLocalPlayer: {IsLocalPlayer}");
    
            // Send player name to server if this is the local player
            if (IsLocalPlayer && !IsHost)
            {
                var playerName = AuthenticationManager.Instance?.PlayerName ?? "Unknown";
                if (_enableDebugLog)
                    Debug.Log($"NetworkPlayer:Multiplayer-Sending player name '{playerName}' to server");
                SetPlayerNameServerRpc(playerName);
            }
    
            // Initialize player data (only server can set NetworkVariables)
            InitializePlayer();
    
            // Register with PlayerRegistry once spawned
            if (!PlayerRegistry.Instance) return;
            if (_enableDebugLog)
                Debug.Log($"NetworkPlayer:Multiplayer-Registering player {OwnerClientId} with PlayerRegistry");
            PlayerRegistry.Instance.RegisterPlayer(this);
        }

        public override void OnNetworkDespawn()
        {
            if (_enableDebugLog)
                Debug.Log($"NetworkPlayer:Multiplayer-NetworkPlayer OnNetworkDespawn - ClientId: {OwnerClientId}");
            
            // Unregister from PlayerRegistry when despawning
            if (PlayerRegistry.Instance != null)
            {
                if (_enableDebugLog)
                    Debug.Log($"NetworkPlayer:Multiplayer-Unregistering player {OwnerClientId} from PlayerRegistry");
                PlayerRegistry.Instance.UnregisterPlayer(this);
            }
            
            base.OnNetworkDespawn();
        }

        public  virtual void SetCharacterClassVisualsLocally(int characterId)
        {
            if (_enableDebugLog)
                Debug.Log($"NetworkPlayer:Multiplayer-SetCharacterClassVisualsLocally called for ClientId: {OwnerClientId} and the character class id: {characterId}");
            // Implement character class visuals logic here (e.g., change model, materials, etc.)
        }

        public virtual void EnablePlayerToStartGame()
        { 
            if (_enableDebugLog)
                Debug.Log($"NetworkPlayer:Multiplayer-EnablePlayerToStartGame called for ClientId: {OwnerClientId}");
            // Implement player enabling logic here (e.g., enable character controller, visuals, etc.)
        }

        public void SetCharacterSelection(int characterId, string characterName)
        {
            CharacterId.Value = characterId;
            CharacterName.Value = characterName;
        }

        void InitializePlayer()
        {
            if (!IsServer) return; // only server can set NetworkVariables!
    
            if (_enableDebugLog)
                Debug.Log($"NetworkPlayer:Multiplayer-InitializePlayer called - ClientId: {OwnerClientId}, IsServer: {IsServer}");
    
            // For host player, set name immediately
            if (IsLocalPlayer)
            {
                var playerName = AuthenticationManager.Instance?.PlayerName ?? "Unknown";
                PlayerName.Value = playerName;
                gameObject.name = "NetworkPlayer (" + PlayerName.Value + ")";
        
                if (_enableDebugLog)
                    Debug.Log($"NetworkPlayer:Multiplayer-Set host player name to '{playerName}'");
            }
            // For remote players, the name will be set via SetPlayerNameServerRpc
    
            var playerId = PlayerConnectionsManager.Instance.GetPlayerId(OwnerClientId);
            PlayerId.Value = playerId;
    
            PlayerConnectionsManager.Instance.RegisterPlayerAndRemoveDuplicates(this);

            if (_enableDebugLog)
                Debug.Log($"NetworkPlayer:Multiplayer-Player initialization completed for {OwnerClientId}: {PlayerName.Value}. PlayerId={PlayerId.Value}");
        }

        [ServerRpc(RequireOwnership = false)]
        void SetPlayerNameServerRpc(string playerName, ServerRpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;
    
            if (_enableDebugLog)
                Debug.Log($"NetworkPlayer:Multiplayer-Received SetPlayerNameServerRpc from client {clientId} with name '{playerName}'");
    
            if (OwnerClientId != clientId) return;
            PlayerName.Value = playerName;
            gameObject.name = "NetworkPlayer (" + PlayerName.Value + ")";
        
            if (_enableDebugLog)
                Debug.Log($"NetworkPlayer:Multiplayer-Set PlayerName to '{playerName}' for ClientId {clientId}");
        }
    }
}
