using UnityEngine;

public class PivotHandler : MonoBehaviour
{
    [Tooltip("This is only applicable on object that contains Sway script")]
    [SerializeField] Transform PivotObject;
    [Tooltip("This object only offsets the mesh to balance out with PivotObject \n" +
             "Object should have no scripts and be seperate from effects")]
    [SerializeField] Transform OffsetObject;

    private void Awake()
    {
        PivotObject.localPosition += transform.localPosition;
        OffsetObject.localPosition -= transform.localPosition;

        transform.localPosition = Vector3.zero;
    }
}
