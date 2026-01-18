using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Splines;

public class Manager_Cameras : MonoBehaviour, IGameManager
{
    public ManagerStatus Status { get; private set; }

    [Header("Cameras")]
    [SerializeField] CinemachineBrain brain;
    [SerializeField] SplineContainer dollySpline;
    [SerializeField] GameObject cameraSlots;
    [SerializeField] GameObject cameraFuses;
    [SerializeField] GameObject cameraMain;
    BezierKnot knot0, knot1;

    [Header("Frames")]
    [SerializeField] RectTransform frameSlotsBottom;
    [SerializeField] RectTransform frameSlotsTop;
    [SerializeField] RectTransform frameFusesBottom;
    [SerializeField] RectTransform frameFusesTop;

    public void SetupValues()
    {
        Status = ManagerStatus.Starting;

        knot0 = dollySpline.Spline.ToArray()[0];
        knot1 = dollySpline.Spline.ToArray()[1];
        UpdateFrame(CameraType.Slots);
        UpdateFrame(CameraType.Fuses);
        UpdateFrame(CameraType.Main);

        Status = ManagerStatus.Activated;
    }

    public void SwitchCamera(CameraType cam, float speed)
    {
        brain.DefaultBlend.Time = speed;

        if (cam == CameraType.Main)
        {
            cameraMain.GetComponent<Camera_Move>().ResetDollyPosition();
            cameraMain.SetActive(false);
            cameraMain.SetActive(true);
        }
        else if (cam == CameraType.Fuses)
        {
            cameraFuses.SetActive(false);
            cameraFuses.SetActive(true);
        }
        else if (cam == CameraType.Slots)
        {
            cameraSlots.SetActive(false);
            cameraSlots.SetActive(true);
        }
    }

    public void UpdateFrame(CameraType cam)
    {
        if (cam == CameraType.Main)
        {
            knot0.Position = dollySpline.transform.InverseTransformPoint(frameSlotsBottom.position);
            knot0.Position.z = -10;
            knot1.Position = dollySpline.transform.InverseTransformPoint(frameFusesTop.position);
            knot1.Position.z = -10;

            dollySpline.Spline.SetKnot(0, knot0);
            dollySpline.Spline.SetKnot(1, knot1);
        }
        else if (cam == CameraType.Fuses)
        {
            frameFusesBottom.anchoredPosition = frameSlotsTop.anchoredPosition;

            if (MainManager.Turn.Fuses.Count > 2)
            {
                Vector3 topPos = MainManager.Turn.Fuses.Peek().GetComponent<RectTransform>().position;
                topPos.y += 1;
                frameFusesTop.position = topPos;
            }
            else
            {
                Vector3 tempPosition = frameFusesBottom.anchoredPosition;
                tempPosition.y += 1050;
                frameFusesTop.anchoredPosition = tempPosition;
            }
        }
        else if (cam == CameraType.Slots)
        {
            frameSlotsTop.anchoredPosition = MainManager.TableCards.SlotBoss.position;
        }
    }
}
