using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    //Use properties for Singleton pattern
    //All classes can get, only Player class can set
    public static Player Instance { get; private set; }

    //2:55 on video, EventArgs and EventHandler
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs{
        public ClearCounter selectedCounter;
    }
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;

    private bool isWalking;
    private Vector3 lastInteractDir;
    //serialized field opens var to editor without opening it to other scripts
    private ClearCounter selectedCounter;

    private void Start(){
        gameInput.OnInteractAction += GameInput_OnInteractAction;
    }

    private void Awake(){
        if (Instance != null){
            Debug.LogError("There is more than one player Instance");
        }
        Instance = this;
    }
    private void GameInput_OnInteractAction(object sender, System.EventArgs e){
        if (selectedCounter != null){
            selectedCounter.Interact();
        
       }    
    }
    private void Update(){
        HandleMovement();
        HandleInteractions();
    }

    //IsWalking just returns isWalking, and is used for animation handling
    public bool IsWalking(){
        return isWalking;
    }

    private void HandleInteractions(){
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        
        if (moveDir != Vector3.zero){
            lastInteractDir = moveDir;
        }
        
        float interactDistance = 2f;
        //RaycastAll returns array of hit, Raycast returns first, Layermask only returns hit on layer
        if(Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask)){
            if(raycastHit.transform.TryGetComponent(out ClearCounter clearCounter)){
                //has ClearCounter
                if (clearCounter != selectedCounter){
                    //Change selected counter (player is looking at a diff counter now)
                    SetSelectedCounter(clearCounter);
                }
            } else {
                //raycastHit hits an object that is not a counter
                SetSelectedCounter(null);
            }
        } else {
            //raycastHit hits no object whatsoever
            SetSelectedCounter(null);
        }
        
    }

    private void HandleMovement(){
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        
        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        
        //Honestly really dont like raycast for this
        //See if we can use/make hitboxes later or research how others
        //bool canMove = !Physics.Raycast(transform.position, moveDir, playerSize);
        
        //TODO: works better, still wanna make hitboxes later
        /*OTHER OPTION:
            Define moveDir =  angle of reflection
        */

        //2:17, rec's other collision videos
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if(!canMove){

            //attempt only X movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0 ,0); //.normalized; I prefer it to move slower when pushing at an angle
            canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);
            if(canMove){
                //set X movement to moveDir
                moveDir = moveDirX;
            }
            else{

                //attempt only Z movement
                Vector3 moveDirZ = new Vector3(0,0,moveDir.z);
                canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);
                if(canMove){
                    //set Z movement to moveDir
                    moveDir = moveDirZ;
                }
                else{
                    //cannot move in any direction
                } 
            }

        }

        if(canMove){
            transform.position += moveDir * moveDistance; //mult by time so time is not depend on framerate
        }
        isWalking = moveDir != Vector3.zero;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp (transform.forward, moveDir, Time.deltaTime*rotateSpeed);
    }

    private void SetSelectedCounter(ClearCounter selectedCounter){
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs{
            selectedCounter = selectedCounter //construct new Args using our class (counter object)
        });
    }

}
