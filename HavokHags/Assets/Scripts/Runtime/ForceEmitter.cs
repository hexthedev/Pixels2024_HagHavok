using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hags
{
    public class ForceEmitter : MonoBehaviour
    {
        float t = 0;
        
        // Update is called once per frame
        void Update()
        {
            if (t >= 3)
            {
                foreach (Rigidbody2D rb in GameObject.FindObjectsOfType<Rigidbody2D>())
                {
                    Debug.Log(rb.name);
                }

                t = 0;
            }

            t += Time.deltaTime;
        }
    }
}
