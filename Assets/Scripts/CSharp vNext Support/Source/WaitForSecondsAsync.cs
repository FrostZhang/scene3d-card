using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WaitForSecondsAsync : CustomYieldInstruction
{
    float end;
    float seconds;
    CancellationToken tokenSourceUnity = default;
    public override bool keepWaiting
    {
        get
        {
            if (tokenSourceUnity.IsCancellationRequested)
            {
                return false;
            }
            else if (end >= Time.time)
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// Use CancellationTokenSourceUnity get token
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="tokenSourceUnity"></param>
    public WaitForSecondsAsync(float seconds, CancellationToken tokenSourceUnity = default)
    {
        end = Time.time + seconds;
        this.tokenSourceUnity = tokenSourceUnity;
    }

    public override void Reset()
    {
        end = Time.time + seconds;
    }
}
