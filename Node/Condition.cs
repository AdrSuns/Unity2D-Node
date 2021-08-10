using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition
{
    public float localTime = 0f;
    public float period;
    public int timeUpperBound = 0;
    private int timeExecuted = 0;
    private bool haveTimeUB;
    private Actions.Exit exitFunc = () => { return false; };
    public Condition(float _period, int timeUB, float startOffset = 0f)
    {
        localTime += startOffset;
        period = _period;
        if (timeUB >= 1)
        {
            haveTimeUB = true;
            timeUpperBound = timeUB;
        }
        else haveTimeUB = false;
    }
    public void setExitFunc(Actions.Exit func)
    {
        exitFunc = func;
    }
    public bool excutable()
    {
        if (filled())
        {
            resetTime(); return true;
        }
        return false;
    }
    public void updateTime(bool useRealTime = false)
    {
        localTime += (useRealTime) ? GameControl.instance.realDeltaTime : Time.deltaTime;
    }
    public bool delete()
    {
        return exitFunc() || timesEnough();
    }
    public void reset()
    {
        resetTime(); timeExecuted = 0;
    }
    public void executedOnce()
    {
        if (haveTimeUB) timeExecuted++;
    }
    private void resetTime()
    {
        localTime = 0f;
    }
    private bool filled()
    {
        return localTime >= period;
    }
    private bool timesEnough()
    {
        return haveTimeUB && (timeExecuted >= timeUpperBound);
    }
    public void debug()
    {
    }
}
