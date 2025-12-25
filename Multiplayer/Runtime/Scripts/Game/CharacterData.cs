using System;
using Unity.Collections;

namespace MidniteOilSoftware.Multiplayer
{
    public struct CharacterData : IEquatable<CharacterData>
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
    }
}
