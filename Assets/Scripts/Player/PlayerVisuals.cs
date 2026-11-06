using UnityEngine;

namespace StarterAssets
{
    public class PlayerVisuals : MonoBehaviour
    {
        [Header("Limb Transforms")]
        public Transform LeftArm;
        public Transform RightArm;
        public Transform LeftLeg;
        public Transform RightLeg;
        public Transform Head;
        public Transform Backpack;

        private ThirdPersonController _controller;
        private float _walkCycle = 0.0f;

        private void Start()
        {
            _controller = GetComponentInParent<ThirdPersonController>();
        }

        private void Update()
        {
            float speed = _controller != null ? _controller.GetCurrentSpeed() : 0.0f;
            bool isGrounded = _controller != null ? _controller.IsGrounded() : true;

            if (isGrounded && speed > 0.15f)
            {
                // Walking / running limb animation
                float cycleSpeed = speed * 3.8f;
                _walkCycle += Time.deltaTime * cycleSpeed;

                float legSwing = Mathf.Sin(_walkCycle) * 38.0f;
                float armSwing = -Mathf.Sin(_walkCycle) * 38.0f;

                if (LeftLeg != null) LeftLeg.localRotation = Quaternion.Euler(legSwing, 0f, 0f);
                if (RightLeg != null) RightLeg.localRotation = Quaternion.Euler(-legSwing, 0f, 0f);
                if (LeftArm != null) LeftArm.localRotation = Quaternion.Euler(armSwing, 0f, 0f);
                if (RightArm != null) RightArm.localRotation = Quaternion.Euler(-armSwing, 0f, 0f);

                // Subtle head bob
                if (Head != null) Head.localPosition = new Vector3(0f, 1.55f + Mathf.Abs(Mathf.Sin(_walkCycle)) * 0.04f, 0f);
            }
            else if (!isGrounded)
            {
                // In-air jump pose
                if (LeftLeg != null) LeftLeg.localRotation = Quaternion.Slerp(LeftLeg.localRotation, Quaternion.Euler(-30f, 0f, 0f), Time.deltaTime * 12f);
                if (RightLeg != null) RightLeg.localRotation = Quaternion.Slerp(RightLeg.localRotation, Quaternion.Euler(30f, 0f, 0f), Time.deltaTime * 12f);
                if (LeftArm != null) LeftArm.localRotation = Quaternion.Slerp(LeftArm.localRotation, Quaternion.Euler(-50f, 0f, -15f), Time.deltaTime * 12f);
                if (RightArm != null) RightArm.localRotation = Quaternion.Slerp(RightArm.localRotation, Quaternion.Euler(-50f, 0f, 15f), Time.deltaTime * 12f);
            }
            else
            {
                // Idle breathing motion
                float breathe = Mathf.Sin(Time.time * 2.2f) * 2.5f;
                if (LeftLeg != null) LeftLeg.localRotation = Quaternion.Slerp(LeftLeg.localRotation, Quaternion.identity, Time.deltaTime * 8f);
                if (RightLeg != null) RightLeg.localRotation = Quaternion.Slerp(RightLeg.localRotation, Quaternion.identity, Time.deltaTime * 8f);
                if (LeftArm != null) LeftArm.localRotation = Quaternion.Slerp(LeftArm.localRotation, Quaternion.Euler(breathe, 0f, 0f), Time.deltaTime * 8f);
                if (RightArm != null) RightArm.localRotation = Quaternion.Slerp(RightArm.localRotation, Quaternion.Euler(-breathe, 0f, 0f), Time.deltaTime * 8f);
                if (Head != null) Head.localPosition = Vector3.Lerp(Head.localPosition, new Vector3(0f, 1.55f, 0f), Time.deltaTime * 8f);
            }
        }
    }
}
