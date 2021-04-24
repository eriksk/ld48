using UnityEngine;

namespace LD48.Game.Players
{
    public class PlayerController : MonoBehaviour
    {
        public Rigidbody2D Rigidbody;
        public Animation Animator;

        private string _animation;

        void Start()
        {

        }

        void PlayAnimation(string name, float transition)
        {
            if (_animation == name) return;
            _animation = name;

            Animator.CrossFade(_animation, transition);
        }

        void Update()
        {
            var movementInput = new Vector2(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical")
            );

            if (Mathf.Abs(movementInput.x) > 0.3f)
            {
                PlayAnimation("walk", 0.2f);
            }
            else
            {
                PlayAnimation("idle", 0.2f);
            }

        }

        void FixedUpdate()
        {

        }
    }
}