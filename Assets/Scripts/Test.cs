using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class Test: MonoBehaviour
    {
        public Transform target;
        public TMPro.TextMeshProUGUI pos;
        public TMPro.TextMeshProUGUI active;
        private Rigidbody2D rb;

        private void Start()
        {
            rb = target.gameObject.GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            pos.text = target.position.ToString();
            active.text = target.gameObject.activeInHierarchy.ToString();
        }
    }
    
    
    // 1.98   21.67  0.06  - улетела сюда    осталась видимой
    // 1.52   21.62  0.06 
    
    //         - 3.55      - стартовая
}