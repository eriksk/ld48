using System;
using System.Collections.Generic;
using UnityEngine;

namespace LD48
{
    [ExecuteInEditMode]
    public class CyclicCoordinateDescent : MonoBehaviour
    {
        public int Iterations = 5;

        [Range(0.01f, 1)]
        public float Damping = 1;

        public Transform Target;
        public Transform EndTransform;

        public Node[] AngleLimits = new Node[0];

        private Dictionary<Transform, Node> _cache;

        [Serializable]
        public class Node
        {
            public Transform Transform;
            public float min;
            public float max;
        }

        private void Start()
        {
            _cache = new Dictionary<Transform, Node>(AngleLimits.Length);
            foreach (var node in AngleLimits)
                if (!_cache.ContainsKey(node.Transform))
                    _cache.Add(node.Transform, node);
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
                Start();

            if (Target == null || EndTransform == null)
                return;

            int i = 0;

            while (i < Iterations)
            {
                CalculateIK();
                i++;
            }

            EndTransform.rotation = Target.rotation;
        }

        private void CalculateIK()
        {
            Transform node = EndTransform.parent;

            while (true)
            {
                RotateTowardsTarget(node);

                if (node == transform)
                    break;

                node = node.parent;
            }
        }

        private void RotateTowardsTarget(Transform transform)
        {
            var toTarget = Target.position - transform.position;
            var toEnd = EndTransform.position - transform.position;

            var angle = SignedAngle(toEnd, toTarget);
            angle *= Mathf.Sign(transform.root.localScale.x);
            angle *= Damping;
            angle = -(angle - transform.eulerAngles.z);

            if (_cache.ContainsKey(transform))
            {
                var node = _cache[transform];
                float parentRotation = transform.parent ? transform.parent.eulerAngles.z : 0;
                angle -= parentRotation;
                angle = ClampAngle(angle, node.min, node.max);
                angle += parentRotation;
            }

            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        public static float SignedAngle(Vector3 a, Vector3 b)
        {
            float angle = Vector3.Angle(a, b);
            float sign = Mathf.Sign(Vector3.Dot(Vector3.back, Vector3.Cross(a, b)));

            return angle * sign;
        }

        private float ClampAngle(float angle, float min, float max)
        {
            angle = Mathf.Repeat(angle, 360);
            min = Mathf.Repeat(min, 360);
            max = Mathf.Repeat(max, 360);
            bool inverse = false;
            var tmin = min;
            var tangle = angle;
            if (min > 180)
            {
                inverse = !inverse;
                tmin -= 180;
            }
            if (angle > 180)
            {
                inverse = !inverse;
                tangle -= 180;
            }
            var result = !inverse ? tangle > tmin : tangle < tmin;
            if (!result)
                angle = min;
            inverse = false;
            tangle = angle;
            var tmax = max;
            if (angle > 180)
            {
                inverse = !inverse;
                tangle -= 180;
            }
            if (max > 180)
            {
                inverse = !inverse;
                tmax -= 180;
            }

            result = !inverse ? tangle < tmax : tangle > tmax;
            if (!result)
                angle = max;
            return angle;
        }
    }
}