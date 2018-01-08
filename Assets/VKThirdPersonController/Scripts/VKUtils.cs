using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VKUtils
{
    public class VKTimer
    {
        public float reachTime;
        public float accumTime;

        public VKTimer(float reachTime)
        {
            this.accumTime = reachTime;
            this.reachTime = reachTime;
        }

        public bool IsAlive()
        {
            if (accumTime < reachTime)
            {
                accumTime += Time.deltaTime / reachTime;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            this.accumTime = 0f;
        }

        public void Reset(float reachTime)
        {
            this.accumTime = 0f;
            this.reachTime = reachTime;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        do
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
        } while (angle < -360 || angle > 360);

        return Mathf.Clamp(angle, min, max);
    }
}
