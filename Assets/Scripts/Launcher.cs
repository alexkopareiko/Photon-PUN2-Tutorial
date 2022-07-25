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
        
        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
        /// we need to keep track of this to properly adjust the behaviour when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        bool isConnecting;


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
            // we don't want to do anything if we are not attempting to join a room
            // this case where isConnecting is false is typically when you lost or quit the game,
            // when this level is loaded, onConnetedToMaster will be called, in that case we don't want to do anything
            if(isConnecting) {
                // #Crytical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
                PhotonNetwork.JoinRandomRoom();
                isConnecting = false;
            }
            
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: onDisconnected() was called by PUN with reason {0}", cause);
            isConnecting = false;

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

            // #Cricitcal: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
            if(PhotonNetwork.CurrentRoom.PlayerCount == 1) {
                Debug.Log("We load the 'Room fo 1'");

                // #Critical
                // Load the Room Level
                PhotonNetwork.LoadLevel("Room for 1");
            }
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
                // keep track of the will to join a room, because we come back from the game we will get a callback that we are connected, 
                // so we need to know what to do then
                isConnecting = PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        #endregion


    }
}

