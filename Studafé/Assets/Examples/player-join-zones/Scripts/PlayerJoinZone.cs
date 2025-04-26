    using System.Text;
    using UdonSharp;
    using UnityEngine;
    using VRC.SDK3.Data;
    using VRC.SDKBase;
    using VRC.Udon;
    using VRC.Udon.Common.Interfaces;

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerJoinZone : UdonSharpBehaviour
    {
        // Modes, could be enums
        protected const int MODE_JOIN = 0;
        protected const int MODE_GAME = 1;
        protected const int MODE_END = 2;
        
        protected DataList Players = new DataList();

        [SerializeField]
        protected UdonBehaviour[] targets;
        
        // Mode
        [UdonSynced, FieldChangeCallback(nameof(Mode))]
        private int _mode = MODE_JOIN;
        public int Mode
        {
            set
            {
                _mode = value;

                // Reset Players for Owner
                if (_mode == MODE_JOIN && Networking.IsOwner(gameObject))
                {
                    ResetPlayers();
                }
                OnModeChanged();
            }
            get => _mode;
        }
        
        private void Start()
        {
            // Sets up the initial state
            Mode = MODE_JOIN;
            RequestSerialization();
            // Start-like hook for extended classes
            PostStart();
        }

        // Clears out _players
        private void ResetPlayers()
        {
            Players = new DataList();
            OnPlayersChanged();
        }

        // Called from UI
        public void _ToggleMode()
        {
            if (!Networking.IsOwner(gameObject))
            {
                SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(ToggleModeRPC));
            }
            else
            {
                ToggleModeRPC();
            }
        }

        public virtual void ToggleModeRPC()
        {
            switch (Mode)
            {
                case MODE_JOIN:
                    Mode = MODE_GAME;
                    break;
                case MODE_GAME:
                    Mode = MODE_END;
                    break;
                case MODE_END:
                    Mode = MODE_JOIN;
                    break;
            }
            RequestSerialization();
        }

        protected void SendEventToAllTargets(string eventName)
        {
            foreach (var target in targets)
            {
                target.SendCustomEvent(eventName);
            }
        }

        // Stay used instead of Enter, this covers some edge cases like changing modes with players already in the zone
        public override void OnPlayerTriggerStay(VRCPlayerApi player)
        {
            if (!ShouldTrackPlayers()) return;
            
            var playerToken = new DataToken(player);
            
            // Exit if player is already in listing
            if (Players.Contains(playerToken)) return;
            
            Players.Add(playerToken);
            OnPlayersChanged();
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!ShouldTrackPlayers()) return;
            
            // Remove the player from the array
            Players.Remove(new DataToken(player));
            
            OnPlayersChanged();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            // Always try to remove players if they leave the instance
            if (Networking.IsOwner(gameObject) && Players != null)
            {
                var token = new DataToken(player);
                if (Players.Contains(token))
                {
                    Players.Remove(new DataToken(player));
                    OnPlayersChanged();
                }
            }
        }

        private bool ShouldTrackPlayers()
        {
            // Only run logic for the owner of the gameObject, when in join mode
            return Mode == MODE_JOIN && Networking.IsOwner(gameObject);
        }
        
        //获取玩家，数组格式
        public VRCPlayerApi[] GetPlayersInZone()
        {
            var players = new VRCPlayerApi[Players.Count];
            for (int i = 0; i < Players.Count; i++)
            {
                players[i] = (VRCPlayerApi)Players[i].Reference;
            }
            return players;
        }    

        protected string GetPlayersAsStringList()//获取player，string格式
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Players.Count; i++)
            {
                // only add non-empty names
                var targetPlayer = (VRCPlayerApi)Players[i].Reference;
                if (!Utilities.IsValid(targetPlayer)) continue;
            
                // Add a comma if we've already added a name
                if (sb.Length > 0) sb.Append(",");
                // Add this player's name
                sb.Append(targetPlayer.displayName);
            }
            return sb.ToString();
        }
        
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (!player.IsValid())
            {
                Debug.LogError($"Ownership transferred to Invalid player!");
                return;
            }

            if (player.isLocal)
            {
                Mode = MODE_JOIN;
                RequestSerialization();
            }
        }

        #region Virtual Methods
        
        public virtual void OnPlayersChanged()
        {
            // Only the owner should run this logic
            if (!Networking.IsOwner(gameObject)) return;
            
            // Propagate Player changes to everyone
            foreach (var target in targets)
            {
                target.SetProgramVariable(nameof(Players), Players);
            }
            // Players is a Datalist, which does not trigger OnVariableChanged events, so we propagate its event manually
            SendEventToAllTargets(nameof(OnPlayersChanged));
            
            // Reset if we have 0 players outside of MODE_JOIN
            if (Mode != MODE_JOIN && Players.Count == 0)
            {
                Mode = MODE_JOIN;
                RequestSerialization();
            }
        }
        
        public virtual void OnModeChanged()
        {
            foreach (var target in targets)
            {
                target.SetProgramVariable(nameof(Mode), Mode);
            }
        }

        public bool IsPlayerInside(VRCPlayerApi player)
    {
        // 假设Players为DataList，可遍历判断player.displayName是否存在
        for (int i = 0; i < Players.Count; i++)
        {
            VRCPlayerApi p = (VRCPlayerApi)Players[i].Reference;
            if (p != null && p.playerId == player.playerId)
                return true;
        }
        return false;
    }

        
        public virtual void PostStart() { }

        #endregion
    }
