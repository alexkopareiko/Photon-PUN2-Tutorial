using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Com.NikfortGames.MyGame {
    public class    Launcher : MonoBehaviourPunCallbacks
    {

        #region Private Serializable Fields

        #endregion

        #region Private Fields


        /// <summary>
        /// This client's version nubmer. Users are separated from each other by gameVesrion
        /// which allows to make breaking changes
        /// </summary>  
        string gameVersion = "1";

        /// <summary>
        /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
        /// </summary>  
        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.")]
        [SerializeField] private byte maxPlayersPerRoom = 4;

        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        [SerializeField] private GameObject controlPanel;
        [Tooltip("The Ui Label to inform the user that the connection  is in progress")]
        [SerializeField] private GameObject progressLabel;


        #endregion


        #region MonoBehaviour CallBacks

        
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase
        /// </summary>
        private void Awake() {
            // #Critical
            // this makes sure user PhotonNetwork.LoadLevel() on the master client and all clients on the same room sync their level automaticaly
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase
        /// </summary>
        private void Start() {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: onDisconnected() was called by PUN with reason {0}", cause);

        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

            // #Crtitical: we failed to join a random room, maybe none existis or they are all full. Create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() was called by PUN. Now this client is in a room.");
        }


        #endregion


        #region Public Methods

        /// <summary>
        /// Start the connection process.
        /// if already connected. we attempt joining a random room
        /// if not yet connected, Connect this aplicaiton instance to Photon Cloud Network
        /// </summary>
        public void Connect() {
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);
            // we check if we are connected or not, we join if we are, else we initiate the connection to server
            if(PhotonNetwork.IsConnected) {
                // #Critical we need at this point to atempt joining a Random Room. 
                // if it fails, we'll get notified in OnJoinRandomFailed() and we'll create one
                PhotonNetwork.JoinRandomRoom();
            }
            else {
                // #Critical, we must first and foremost connect to Photon Online Server
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        #endregion


    }
}

