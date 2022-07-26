using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

namespace Com.NikfortGames.MyGame{
    /// <summary>
    /// Player manager.
    /// Handles fire input and Beams.
    /// </summary> 
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if(stream.IsWriting) {
                // we own this player: send the others our data
                stream.SendNext(IsFiring);
                stream.SendNext(Health);
            }
            else {
                // Network player, receive data
                this.IsFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
            }
        }

        #endregion

        #region Public Fields

        [Tooltip("The local player instance. Use this to know if the local player is reprsented in the Scene")]
        public static GameObject LocalPlayerInstance;

        [Tooltip("The current Health of our Player")]
        public float Health = 1f;

        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField] public GameObject PlayerUiPrefab;

        #endregion

        #region Private Fields

        [Tooltip("The Beams GameObject to control")]
        [SerializeField] private GameObject beams;
        /// True, when user is firing
        bool IsFiring;

        #endregion
        

        #region MonoBehaviour Callbacks
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary> 
        private void Awake() {
            if(beams == null) {
                Debug.LogError("<Color=Red><a>Missing</a></Color> Beams reference", this);
            } else {
                beams.SetActive(false);
            }
            // #important
            // used in GameManager.cs: we keep track of the localPlayerinstance to preent instantiation when levels are synchronized
            if(photonView.IsMine){
                PlayerManager.LocalPlayerInstance = this.gameObject;
            }
            // #critical
            // we flag as don't destroy on load so that instance survices level synchronization, thus giving a seamless experience when levels load
            DontDestroyOnLoad(this.gameObject);
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phaze.
        /// </summary> 
        private void Start() {
            CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();

            if(_cameraWork != null) {
                if( photonView.IsMine) {
                    _cameraWork.OnStartFollowing();
                }
            }
            else {
                Debug.LogError("<Color=Red><a>Missing</a></Color> Camera Work Component on playerPrefab", this);
            }
            #if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. Register a method to callCalledOnLevelWasLoaded
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            #endif

            if(PlayerUiPrefab != null) {
                GameObject _uiGo = Instantiate(PlayerUiPrefab);
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab", this);

            }
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary> 
        private void Update() {
            if(photonView.IsMine) {
                ProcessInputs();
            }
            if(Health <= 0f) {
                GameManager.Instance.LeaveRoom();
            }

            //trigger Beams active state
            if(beams != null && IsFiring != beams.activeInHierarchy) {
                beams.SetActive(IsFiring);
            }
        }

        /// <summary>
        /// MonoBehaviour method called when Collider 'other' enters the trigger.
        /// AffectHealth of the Player if the collider is a beam
        /// Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
        /// One could move the collider further away to prevent this or check if the beam belongs to the player
        /// </summary> 
        private void OnTriggerEnter(Collider other) {
            if(!photonView.IsMine) {
                return;
            }
            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name
            if(!other.name.Contains("Beam")){
                return;
            }
            Health -= 0.1f;
        }

        /// <summary>
        /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
        /// We're going to affect health while the beams are touching the player
        /// </summary>
        /// <param name="other">Other.</param>
        private void OnTriggerStay(Collider other) {
            // we don;t do anything if we are not the local player
            if(!photonView.IsMine) {
                return;
            }
            // we are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name
            if(!other.name.Contains("Beam")){
                return;
            }
            // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
            Health -= 0.1f * Time.deltaTime;
        }

        #if UNITY_5_4_OR_NEWER
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode){
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
        #endif

        #if UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4</summary>
        void OnLevelWasLoaded(int level) {
            this.CalledOnLevelWasLoaded(level);
        }
        #endif

        void CalledOnLevelWasLoaded(int level) {
            // check if we are outsed the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if(!Physics.Raycast(transform.position, -Vector3.up, 5f)) {
                transform.position = new Vector3(0f, 5f, 0f);
            }
            GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }

        #if UNITY_5_4_OR_NEWER
        public override void OnDisable() {
            // Always call the base to remove callbacks
            base.OnDisable();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endif

        #endregion

        #region Custom

        /// <summary>
        /// Process the inputs. Maintain a flag representing when the user is pressing Fire.
        /// </summary> 
        void ProcessInputs() {
            if(Input.GetButtonDown("Fire1")){
                if(!IsFiring) {
                    IsFiring = true;
                }
            }
            if(Input.GetButtonUp("Fire1")) {
                if(IsFiring) {
                    IsFiring = false;
                }
            }
        }

        #endregion

    }
}


