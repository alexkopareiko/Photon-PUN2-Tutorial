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

        [Tooltip("The current Health of our Player")]
        public float Health = 1f;

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


