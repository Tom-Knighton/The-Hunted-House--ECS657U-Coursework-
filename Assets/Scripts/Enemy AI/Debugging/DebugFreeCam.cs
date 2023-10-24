using System;
using UnityEngine;

namespace Enemy_AI.Debugging
{
    public class DebugFreeCam : MonoBehaviour
    {
        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            var moveFaster = Input.GetKey(KeyCode.LeftShift);
            var speed = moveFaster ? 100 : 10;

            var currentTransform = transform;

            if (Input.GetKey(KeyCode.A))
            {
                currentTransform.position += (-currentTransform.right * (speed * Time.deltaTime));
            }
            if (Input.GetKey(KeyCode.D))
            {
                currentTransform.position += (currentTransform.right * (speed * Time.deltaTime));
            }
            if (Input.GetKey(KeyCode.W))
            {
                currentTransform.position += (currentTransform.forward * (speed * Time.deltaTime));
            }
            if (Input.GetKey(KeyCode.S))
            {
                currentTransform.position += (-currentTransform.forward * (speed * Time.deltaTime));
            }

            var newRotX = currentTransform.localEulerAngles.y + Input.GetAxis("Mouse X") * 3f;
            var newRotY = currentTransform.localEulerAngles.x + Input.GetAxis("Mouse Y") * -3f;
            currentTransform.localEulerAngles = new Vector3(newRotY, newRotX, 0f);

            transform.position = currentTransform.position;
            transform.localEulerAngles = currentTransform.localEulerAngles;
        }
    }
}