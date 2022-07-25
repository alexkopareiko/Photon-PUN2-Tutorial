using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

namespace Com.NikfortGames.MyGame 
{

    /// <summary>
    /// Player name input field. Let the user input his name, will appear above the player in the game.
     /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class PlayerNameInputField : MonoBehaviour
    {
        #region Private Constants

        // Store the PlayerPref Key to avoid typos
        const string playerNamePrefKey = "Playername";

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// Player name input field. Let the user input his name, will appear above the player in the game.
        /// </summary>
        private void Start() {
            
            string defaultName = string.Empty;
            TMP_InputField _tMP_InputField = this.GetComponent<TMP_InputField>();
            if(_tMP_InputField != null) {
                if(PlayerPrefs.HasKey(playerNamePrefKey)) {
                    defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                    _tMP_InputField.text = defaultName;
                }
            }

            PhotonNetwork.NickName = defaultName;
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the name of the player, and save it in the PLayerPrefs for future sessions.
        /// </summary>
        /// <param name="value">The name of the Player</param>
        public void SetPlayerName(string value) {
            // #Important
            if(string.IsNullOrEmpty(value)) {
                Debug.LogError("Player Name is null or Empty");
            }
            PhotonNetwork.NickName = value;

            PlayerPrefs.SetString(playerNamePrefKey, value);
        }
        #endregion  
        
    }
}

