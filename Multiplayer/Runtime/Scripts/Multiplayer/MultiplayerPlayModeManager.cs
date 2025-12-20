using System;
using System.Collections;
using System.Linq;
using MidniteOilSoftware.Core;

using Unity.Services.Authentication;
using UnityEngine;
using System.Threading.Tasks;
using MidniteOilSoftware.Multiplayer.Authentication;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public class MultiplayerPlayModeManager : MonoBehaviour
    {
        [SerializeField] bool _debugLog;
#if UNITY_EDITOR
        public string ProfileName { get; private set; }

        IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);
            yield return StartCoroutine(InitializeMppm());
        }

        const string HostKey = "Host";
        const string ClientKey = "Client";
        const string ServerKey = "Server";
        const string LobbyHostKey = "Lobby Host";
        const string LobbyClientKey = "Lobby Client";

        IEnumerator InitializeMppm()
        {
            if (_debugLog) Debug.Log("MultiplayerPlayModeManager:Multiplayer-InitializeMPPM");
    
            var allTags = Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().ToArray();
            if (_debugLog) Debug.Log($"MultiplayerPlayModeManager:Multiplayer-All tags: [{string.Join(", ", allTags)}]");
    
            ProfileName = Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().Except(new[]
                {
                    LobbyHostKey, LobbyClientKey, HostKey, ClientKey, ServerKey
                })
                .FirstOrDefault();

            if (_debugLog) Debug.Log($"MultiplayerPlayModeManager:Multiplayer-ProfileName extracted: '{ProfileName}'");
    
            if (ProfileName != default)
            {
                AuthenticationService.Instance.SwitchProfile(ProfileName);
            }

            if (_debugLog) Debug.Log($"MultiplayerPlayModeManager:Multiplayer-Starting as {ProfileName}");

            if (Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().Contains(LobbyHostKey))
            {
                if (_debugLog) Debug.Log("MultiplayerPlayModeManager:Multiplayer-Starting Lobby Host Coroutine");
                StartCoroutine(LobbyHost());
            }
            if (Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().Contains(LobbyClientKey))
            {
                if (_debugLog) Debug.Log("MultiplayerPlayModeManager:Multiplayer-Starting Lobby Client Coroutine");
                StartCoroutine(LobbyClient());
            }
            if (Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().Contains(HostKey))
            {
                if (_debugLog) Debug.Log("MultiplayerPlayModeManager:Multiplayer-Starting Host Coroutine");
                StartCoroutine(Host());
            }
            if (Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().Contains(ClientKey))
            {
                if (_debugLog) Debug.Log("MultiplayerPlayModeManager:Multiplayer-Starting Client Coroutine");
                StartCoroutine(Client());
            }
            if (Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().Contains(ServerKey))
            {
                if (_debugLog) Debug.Log("MultiplayerPlayModeManager:Multiplayer-Starting Server Coroutine");
                StartCoroutine(Server());
            }
            yield break;
        }

        IEnumerator Server()
        {
            // Dedicated server is not supported by Multiplayer Services SDK
            Debug.Log("Dedicated server is not supported by Multiplayer Services SDK");
            yield break;
        }

        IEnumerator Client()
        {
            // Direct connection logic, outside of the Multiplayer Services SDK scope
            yield break;
        }

        IEnumerator Host()
        {
            // Direct connection logic, outside of the Multiplayer Services SDK scope
            yield break;
        }

        IEnumerator LobbyClient()
        {
            yield return AuthenticationManager.Instance.SignInAnonymouslyAsync(ProfileName);

            while (PlayerPrefs.HasKey("SessionCode") == false)
            {
                yield return null;
            }

            var sessionCode = PlayerPrefs.GetString("SessionCode");
            if (_debugLog) Debug.Log($"MultiplayerPlayModeManager:Multiplayer-Joining Session with Code {sessionCode}");
            SessionManager.Instance.JoinSessionById(sessionCode);
            yield break;
        }

        IEnumerator LobbyHost()
        {
            yield return AuthenticationManager.Instance.SignInAnonymouslyAsync(ProfileName);
            
            // Pass the session name here
            SessionManager.Instance.StartSessionAsHost(AuthenticationManager.Instance.PlayerName + "'s Game");
            
            while (SessionManager.Instance.ActiveSession == null)
            {
                yield return null;
            }

            var session = SessionManager.Instance.ActiveSession;
            PlayerPrefs.SetString("SessionCode", session.Id);
            if (_debugLog) Debug.Log($"MultiplayerPlayModeManager:Multiplayer-Set SessionCode to {session.Id}");
        }

        void OnDestroy()
        {
            PlayerPrefs.DeleteKey("SessionCode");
        }
#endif
    }
}