
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class PlayerMovement : MonoBehaviour
{
   public float moveSpeed;
   public float jumpForce;
   public Transform playerSeen;
   public float gravity;
   private SaveData saveData;
   
   public float horizontalDrag;
   public float verticalDrag;

   private float jumpCd = 0.1f; // jump cooldown
   private float lastJump = 0f; //time since the last jump
   private Rigidbody rb;
   private bool justBounce;
   public WinUI WinUI;
   bool onSlippery;

   private Vector3 moveDirection = Vector3.zero; // stores direction from Update


   
   
   void Start()
   {
       rb = GetComponent<Rigidbody>(); //assigns the players rb to the variable
       rb.useGravity = false;
       saveData=SaveData.instance;
       rb.linearVelocity = saveData.GetPlayerVelocity();

      Cursor.lockState = CursorLockMode.Locked;
       Cursor.visible = false;
   }


   void Update()
   {
       float inputX = Input.GetAxisRaw("Horizontal"); // left(A)/right(D) will set to -1/1
       float inputZ = Input.GetAxisRaw("Vertical");   // forward(W)/backward(S) will set to -1/1


       //gets the cameras transform component
       Transform cam = Camera.main.transform;


       // gets the cameras forward vector, ingnore y value then normalize, do the same for right
       Vector3 camFw = cam.forward;//z
       camFw.y = 0;
       camFw.Normalize();


       Vector3 camR = cam.right;//x
       camR.y = 0;
       camR.Normalize();


       //combine the camera direction and move inputs, then normalize
       moveDirection = (camFw * inputZ + camR * inputX).normalized;


       //checks if:space is being pressed, the player is toching the ground and the players last jump wasnt too recent
       if (Input.GetKey(KeyCode.Space) && IsGrounded() && Time.time > lastJump + jumpCd)
       {
           //jumps using the jumps force on the player ridgedbody
           rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
           lastJump = Time.time;
       }  
   }


   void FixedUpdate()
   {
       rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
       
       Vector3 verticalVelocity = new Vector3(0f, rb.linearVelocity.y,0f);
       Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x,0f,rb.linearVelocity.z);
       
       float currentHorizontalDrag = horizontalDrag;       
       if (IsGrounded() && onSlippery)
       {
           currentHorizontalDrag = 0f;                       
       }

       
       rb.AddForce(horizontalVelocity*-currentHorizontalDrag, ForceMode.Acceleration);
       rb.AddForce(verticalVelocity*-verticalDrag, ForceMode.Acceleration);
       
       if (IsGrounded())
       {
           if (!justBounce)
           {
               rb.linearVelocity = new  Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
           }
       }
       else
       {
           justBounce=false;
       }
       //checks if the player is touching any movement keys
       if (moveDirection.magnitude > 0)
       {
           MovePlayer(moveDirection);
           //uses the camera direction to set the aimed player rotation and only moves if inputs
           Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
           //slowly rotates the capsule to the target rotation
           playerSeen.rotation = Quaternion.Slerp(playerSeen.rotation, targetRotation, Time.fixedDeltaTime * 10f);
       }
   }
   
   void MovePlayer(Vector3 direction)
   {
       // calculate the new movement vector
       Vector3 move = direction * moveSpeed;
       move.y = rb.linearVelocity.y; //dont change vertical vel
       rb.AddForce(move,ForceMode.Acceleration);
      
   }
   
   bool IsGrounded()
   {
       //cast a raybox down and see if it collides with anything
       return Physics.BoxCast(transform.position, new Vector3(0.48f,0f,0.48f), Vector3.down, out RaycastHit hit , quaternion.identity, 1.01f);
   }

   private void OnCollisionEnter(Collision collision)
   {
       if (collision.gameObject.CompareTag("BouncyPlatform"))
       {
           justBounce = true;
           rb.linearVelocity = new  Vector3(rb.linearVelocity.x, 70f, rb.linearVelocity.z);
       }

       if (collision.gameObject.CompareTag("SlipperyPlatform"))
       {
           onSlippery = true;
       }

       if (collision.gameObject.CompareTag("WinPlatform"))
       {
           WinUI.ReachEnd();
       }
   }
   private void OnCollisionExit(Collision collision)
   {
       if (collision.gameObject.CompareTag("SlipperyPlatform"))
       {
           onSlippery = false; 
       }
   }
}
