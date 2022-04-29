using System;
using System.Threading;
using UnityEngine;

public class WaitUntilAsync : CustomYieldInstruction
{
    Func<bool> predicate;
    CancellationToken tokenSourceUnity = default;
    public override bool keepWaiting
    {
        get
        {
            if (tokenSourceUnity.IsCancellationRequested || predicate == null)
            {
                return false;
            }
            else
            { 
                return !predicate();
            }
        }
    }
    /// <summary>
    /// Use CancellationTokenSourceUnity get token
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="tokenSourceUnity"></param>
    public WaitUntilAsync(Func<bool> predicate, CancellationToken tokenSourceUnity = default)
    {
        this.predicate = predicate;
        this.tokenSourceUnity = tokenSourceUnity;
    }

    public override void Reset()
    {
        predicate = null;
    }
}