using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
  public float speed, sensitivity, jumpForce;
  Rigidbody rb;
  Camera cam;
  void Start(){
    cam = Camera.main;
    rb = GetComponent<Rigidbody>();
  }
  void Update(){ 
    float x = Input.GetAxisRaw("Horizontal") * speed;
    float z = Input.GetAxisRaw("Vertical") * speed;
    Vector3 move = transform.right * x + transform.forward * z;
    Vector3 newPos = rb.position + move * Time.fixedDeltaTime;
    rb.MovePosition(newPos);
    
    float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
    cam.transform.Rotate(Vector3.right * mouseX);
    float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
    cam.transform.Rotate(Vector3.right * mouseY);
    
    if (Input.GetButtonDown("Jump") && Mathf.Abs(rb.velocity.y) < 0.001f)
    {
      rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
  }
}
