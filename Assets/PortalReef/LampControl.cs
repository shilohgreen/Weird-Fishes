using UnityEngine;

/// <summary>
/// A lamp that lives in the room, operated with tracked hands (no controllers).
/// Pinch near the orb to carry it; open the pinch to leave it there.
/// While carrying, pinch with the other hand and drag: up/down = brightness, left/right = hue.
/// </summary>
[RequireComponent(typeof(Light))]
public class LampControl : MonoBehaviour
{
    [Header("Carrying")]
    [SerializeField, Min(0f), Tooltip("The pinching fingertip must be within this distance of the lamp to grab it.")]
    private float grabRadius = 0.12f;
    [SerializeField, Min(0f), Tooltip("Extra reach allowed while only the wrist position is known (before the hand skeleton has loaded).")]
    private float wristFallbackReach = 0.15f;

    [Header("Adjusting with the other hand")]
    [SerializeField, Tooltip("Intensity change per metre of vertical drag.")]
    private float intensityPerMetre = 80f;
    [SerializeField, Tooltip("Turns of the colour wheel per metre of sideways drag.")]
    private float huePerMetre = 1.5f;
    [SerializeField, Min(0f)] private float minIntensity = 0.5f;
    [SerializeField, Min(0f)] private float maxIntensity = 40f;
    [SerializeField, Range(0f, 1f), Tooltip("0 = white light, 1 = fully saturated colour.")]
    private float saturation = 0.45f;

    [Header("Orb")]
    [SerializeField, Tooltip("Material the orb is drawn with. Assign a URP Unlit material so the shader is included in builds.")]
    private Material orbMaterialTemplate;
    [SerializeField, Min(0.01f), Tooltip("Diameter in metres of the visible orb marking the lamp.")]
    private float orbSize = 0.12f;

    [Header("Start position")]
    [SerializeField, Tooltip("On start, move the lamp in front of the viewer's head instead of leaving it at its scene position.")]
    private bool startInFrontOfHead = true;
    [SerializeField, Min(0f), Tooltip("Metres in front of the head.")]
    private float startDistance = 0.7f;
    [SerializeField, Tooltip("Metres below eye level.")]
    private float startDrop = 0.2f;

    private Light lamp;
    private Material orbMaterial;
    private float hue;

    private OVRHand[] hands = new OVRHand[0];
    private bool[] wasPinching = new bool[0];
    private Transform[] indexTips = new Transform[0];

    private OVRHand carryingHand;
    private Vector3 carryOffset;

    private OVRHand adjustingHand;
    private Vector3 adjustStartPosition, adjustRightAxis;
    private float adjustStartIntensity, adjustStartHue;

    private void Awake()
    {
        lamp = GetComponent<Light>();
        Color.RGBToHSV(lamp.color, out hue, out _, out _);
        CreateOrb();
    }

    private void Start()
    {
        if (startInFrontOfHead)
            StartCoroutine(PlaceInFrontOfHead());

        hands = FindObjectsByType<OVRHand>(FindObjectsSortMode.None);
        wasPinching = new bool[hands.Length];
        indexTips = new Transform[hands.Length];
        if (hands.Length == 0)
            Debug.LogWarning("[ 💡 LampControl.Start ] No OVRHand found. Add the Hand Tracking building block so the lamp can be grabbed.");
    }

    private void Update()
    {
        for (int i = 0; i < hands.Length; i++)
        {
            OVRHand hand = hands[i];
            bool pinching = hand.IsTracked && hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            bool pinchStarted = pinching && !wasPinching[i];
            bool pinchEnded = !pinching && wasPinching[i] && hand.IsTracked;

            if (pinchStarted)
                OnPinchStarted(hand, i);
            if (pinchEnded)
                OnPinchEnded(hand);

            // A hand that loses tracking keeps its last state, so a brief dropout does not drop the lamp.
            if (hand.IsTracked)
                wasPinching[i] = pinching;
        }

        if (carryingHand != null && carryingHand.IsTracked)
            transform.position = carryingHand.transform.TransformPoint(carryOffset);

        if (adjustingHand != null && adjustingHand.IsTracked)
            ApplyAdjustment();

        UpdateOrb();
    }

    private void OnPinchStarted(OVRHand hand, int handIndex)
    {
        if (carryingHand == null)
        {
            Transform tip = GetIndexTip(handIndex);
            Vector3 pinchPoint = tip != null ? tip.position : hand.transform.position;
            float reach = tip != null ? grabRadius : grabRadius + wristFallbackReach;

            if (Vector3.Distance(pinchPoint, transform.position) <= reach)
            {
                carryingHand = hand;
                carryOffset = hand.transform.InverseTransformPoint(transform.position);
            }
            return;
        }

        if (hand != carryingHand && adjustingHand == null)
        {
            adjustingHand = hand;
            adjustStartPosition = hand.transform.position;
            adjustStartIntensity = lamp.intensity;
            adjustStartHue = hue;

            // "Sideways" is fixed to the viewer's right at the moment the drag starts.
            Transform head = Camera.main != null ? Camera.main.transform : transform;
            adjustRightAxis = Vector3.ProjectOnPlane(head.right, Vector3.up).normalized;
        }
    }

    /// <summary>
    /// Finds the index fingertip of a hand from its skeleton, as Meta's hand tracking docs describe.
    /// Returns null until the skeleton has initialised; callers then fall back to the wrist.
    /// </summary>
    private Transform GetIndexTip(int handIndex)
    {
        if (indexTips[handIndex] != null)
            return indexTips[handIndex];

        OVRSkeleton skeleton = hands[handIndex].GetComponent<OVRSkeleton>();
        if (skeleton == null || !skeleton.IsInitialized || skeleton.Bones == null)
            return null;

        foreach (OVRBone bone in skeleton.Bones)
        {
            // The id differs between the legacy and OpenXR hand skeletons.
            if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip || bone.Id == OVRSkeleton.BoneId.XRHand_IndexTip)
            {
                indexTips[handIndex] = bone.Transform;
                break;
            }
        }

        return indexTips[handIndex];
    }

    private void OnPinchEnded(OVRHand hand)
    {
        if (hand == adjustingHand)
            adjustingHand = null;

        if (hand == carryingHand)
        {
            carryingHand = null;   // the lamp stays where it is
            adjustingHand = null;  // adjusting only works while the lamp is held
        }
    }

    private void ApplyAdjustment()
    {
        Vector3 drag = adjustingHand.transform.position - adjustStartPosition;

        lamp.intensity = Mathf.Clamp(
            adjustStartIntensity + drag.y * intensityPerMetre, minIntensity, maxIntensity);

        hue = Mathf.Repeat(adjustStartHue + Vector3.Dot(drag, adjustRightAxis) * huePerMetre, 1f);
        lamp.color = Color.HSVToRGB(hue, saturation, 1f);
    }

    /// <summary>
    /// Waits for head tracking to settle, then parks the lamp a short distance in front of the viewer,
    /// so it is always in view on the headset regardless of where the tracking origin landed.
    /// </summary>
    private System.Collections.IEnumerator PlaceInFrontOfHead()
    {
        yield return new WaitForSeconds(1f);

        Camera head = Camera.main;
        if (head == null)
        {
            Debug.LogWarning("[ 💡 LampControl.PlaceInFrontOfHead ] No main camera, leaving the lamp where it is.");
            yield break;
        }

        Vector3 forward = Vector3.ProjectOnPlane(head.transform.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        transform.position = head.transform.position + forward * startDistance + Vector3.down * startDrop;
        Debug.Log($"[ 💡 LampControl.PlaceInFrontOfHead ] Lamp placed at {transform.position}.");
    }

    /// <summary>Builds the small unlit sphere that shows where the lamp is.</summary>
    private void CreateOrb()
    {
        Material template = orbMaterialTemplate;
        if (template == null)
        {
            // Shader.Find only works in a build if the shader shipped, so the assigned material is the reliable path.
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                Debug.LogError("[ 💡 LampControl.CreateOrb ] No orb material assigned and the URP Unlit shader is not in this build.");
                return;
            }
            Debug.LogWarning("[ 💡 LampControl.CreateOrb ] No orb material assigned; falling back to Shader.Find, which can fail in builds.");
            template = new Material(shader);
        }

        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "Lamp Orb";
        Destroy(orb.GetComponent<Collider>()); // fish must not steer around the lamp
        orb.transform.SetParent(transform, false);
        orb.transform.localScale = orbSize * Vector3.one;

        orbMaterial = new Material(template);
        orb.GetComponent<Renderer>().sharedMaterial = orbMaterial;
    }

    /// <summary>Keeps the orb showing the lamp's colour: dimmer when the lamp is dim, brighter while held.</summary>
    private void UpdateOrb()
    {
        if (orbMaterial == null)
            return;

        float level = Mathf.InverseLerp(minIntensity, maxIntensity, lamp.intensity);
        float brightness = Mathf.Lerp(0.7f, 1.2f, level) * (carryingHand != null ? 1.3f : 1f);
        orbMaterial.color = lamp.color * brightness;
    }
}
