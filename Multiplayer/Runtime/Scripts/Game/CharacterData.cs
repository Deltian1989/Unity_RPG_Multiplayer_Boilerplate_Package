using System;
using System.Xml;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor.PackageManager;

namespace MidniteOilSoftware.Multiplayer
{
    public struct CharacterData : INetworkSerializable, IEquatable<CharacterData>
    {
        public ulong clientId;
        public FixedString32Bytes characterName;
        public int characterId;

        public void SetCharacterIdAndName(int characterId, string characterName)
        {
            this.characterId = characterId;
            this.characterName = new FixedString32Bytes(characterName);
        }

        public bool Equals(CharacterData other)
        {
            return clientId == other.clientId &&
                   characterName.Equals(other.characterName) &&
                   characterId == other.characterId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref characterName);
            serializer.SerializeValue(ref characterId);
        }
    }
}
