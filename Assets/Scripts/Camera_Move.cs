using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Camera_Move : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    CinemachineSplineDolly dolly;
    bool isActive = true;

    //Input
    InputAction moveAction;
    float inputVal;

    private void Start()
    {
        dolly = GetComponent<CinemachineSplineDolly>();
        moveAction = InputSystem.actions.FindAction("Move");
    }

    private void Update()
    {
        if (!isActive)
            return;

        Move();
    }

    private void Move()
    {
        inputVal = moveAction.ReadValue<float>() * moveSpeed;
        dolly.CameraPosition += inputVal;
    }

    public void ResetDollyPosition()
    {
        dolly.SplineSettings.InvalidateCache();
        dolly.CameraPosition = 0;
    }
}
