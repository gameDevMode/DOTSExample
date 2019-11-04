using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 10f;
    
    private Rigidbody _rb;

    private float inputX, inputY;

    private Vector3 targetPos;
    // Start is called before the first frame update
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        if(_rb == null)
            return;
        
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");
        if(Input.GetButtonDown("Jump"))
            _rb.AddForce(0f, jumpForce, 0f, ForceMode.Impulse);
        targetPos = _rb.position +
                    new Vector3(inputX * Time.deltaTime * moveSpeed, 0f, inputY * Time.deltaTime * moveSpeed);
        _rb.MovePosition(targetPos);
    }
}
