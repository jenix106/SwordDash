using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace SwordDash
{
    public class SwordDashModule : ItemModule
    {
        public float DashSpeed = 1000f;
        public string DashDirection = "Player";
        public bool DisableGravity = true;
        public bool DisableCollision = false;
        public float DashTime = 0.5f;
        public string ActivationButton = "Alt Use";
        public bool StopOnEnd = false;
        public bool StopOnStart = false;
        public bool ThumbstickDash = false;
        public bool DashRealTime = false;
        public ForceMode DashForceMode = ForceMode.Impulse;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ImprovedComponent>().Setup(DashSpeed, DashDirection, DisableGravity, DisableCollision, DashTime, ActivationButton, StopOnEnd, StopOnStart, ThumbstickDash, DashRealTime, DashForceMode);
        }
    }
    public class ImprovedComponent : MonoBehaviour
    {
        public float DashSpeed;
        public string DashDirection;
        public bool DisableGravity;
        public bool DisableCollision;
        public float DashTime;
        Interactable.Action ActivationButton;
        public bool StopOnEnd;
        public bool StopOnStart;
        bool ThumbstickDash;
        Item item;
        bool fallDamage;
        bool dashing;
        public ForceMode DashForceMode;
        public bool DashRealTime;
        public void Start()
        {
            item = GetComponent<Item>();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
        }
        public void Update()
        {
            if (!dashing) fallDamage = Player.fallDamage;
        }
        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == ActivationButton)
            {
                StopCoroutine(Dash());
                StartCoroutine(Dash());
            }
        }
        public IEnumerator Dash()
        {
            dashing = true;
            Player.fallDamage = false;
            if (StopOnStart) Player.local.locomotion.rb.velocity = Vector3.zero;
            if (Player.local.locomotion.moveDirection.magnitude <= 0 || !ThumbstickDash)
                if (DashDirection == "Item")
                {
                    Player.local.locomotion.rb.AddForce(item.mainHandler.grip.up * (!DashRealTime ? DashSpeed : DashSpeed / Time.timeScale), DashForceMode);
                }
                else
                {
                    Player.local.locomotion.rb.AddForce(Player.local.head.transform.forward * (!DashRealTime ? DashSpeed : DashSpeed / Time.timeScale), DashForceMode);
                }
            else
            {
                Player.local.locomotion.rb.AddForce(Player.local.locomotion.moveDirection.normalized * (!DashRealTime ? DashSpeed : DashSpeed / Time.timeScale), DashForceMode);
            }
            if (DisableGravity)
                Player.local.locomotion.rb.useGravity = false;
            if (DisableCollision)
            {
                Player.local.locomotion.rb.detectCollisions = false;
                item.rb.detectCollisions = false;
                item.mainHandler.rb.detectCollisions = false;
                item.mainHandler.otherHand.rb.detectCollisions = false;
            }
            if (DashRealTime) yield return new WaitForSecondsRealtime(DashTime);
            else yield return new WaitForSeconds(DashTime);
            if (DisableGravity)
                Player.local.locomotion.rb.useGravity = true;
            if (DisableCollision)
            {
                Player.local.locomotion.rb.detectCollisions = true;
                item.rb.detectCollisions = true;
                item.mainHandler.rb.detectCollisions = true;
                item.mainHandler.otherHand.rb.detectCollisions = true;
            }
            if (StopOnEnd) Player.local.locomotion.rb.velocity = Vector3.zero;
            Player.fallDamage = fallDamage;
            dashing = false;
            yield break;
        }

        public void Setup(float speed, string direction, bool gravity, bool collision, float time, string button, bool stop, bool start, bool thumbstick, bool realTime, ForceMode force)
        {
            DashSpeed = speed;
            DashDirection = direction;
            DisableGravity = gravity;
            DisableCollision = collision;
            DashTime = time;
            if (button.ToLower().Contains("trigger") || button.ToLower() == "use")
            {
                ActivationButton = Interactable.Action.UseStart;
            }
            else if (button.ToLower().Contains("alt") || button.ToLower().Contains("spell"))
            {
                ActivationButton = Interactable.Action.AlternateUseStart;
            }
            if (direction.ToLower().Contains("player") || direction.ToLower().Contains("head") || direction.ToLower().Contains("sight"))
            {
                DashDirection = "Player";
            }
            else if (direction.ToLower().Contains("item") || direction.ToLower().Contains("pierce") || direction.ToLower().Contains("flyref") || direction.ToLower().Contains("weapon"))
            {
                DashDirection = "Item";
            }
            StopOnEnd = stop;
            StopOnStart = start;
            ThumbstickDash = thumbstick;
            DashRealTime = realTime;
            DashForceMode = force;
        }
    }
}
