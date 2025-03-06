using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class Test: MonoBehaviour
    {
        public Transform target;
        public TMPro.TextMeshProUGUI pos;
        public TMPro.TextMeshProUGUI active;

        private void Update()
        {
            pos.text = target.position.ToString();
            active.text = target.gameObject.activeInHierarchy.ToString();
        }
    }
}