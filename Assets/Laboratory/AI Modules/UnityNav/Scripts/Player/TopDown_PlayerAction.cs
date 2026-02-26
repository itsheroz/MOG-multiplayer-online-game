using UnityEngine;
using System.Collections;
using Photon.Pun;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class TopDown_PlayerAction : LivingEntity
{

    public float moveSpeed = 5;

    public Camera viewCamera;
    TopDown_PlayerController controller;
    GunController gunController;

    PhotonView photonView;

    protected override void Start()
    {
        base.Start();
        controller = GetComponent<TopDown_PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;

        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        //if(photonView.IsMine)
        //{
            // Movement input
            Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            Vector3 moveVelocity = moveInput.normalized * moveSpeed;
            controller.Move(moveVelocity);

            // Look input
            Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                //Debug.DrawLine(ray.origin,point,Color.red);
                controller.LookAt(point);
            }

            // Weapon input
            if (Input.GetMouseButton(0))
            {
                gunController.Shoot();
            }
        //}
    }
}
