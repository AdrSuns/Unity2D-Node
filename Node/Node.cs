using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Event = System.Collections.Generic.KeyValuePair<Condition, Actions.Func>;

public class Node : MonoBehaviour
{
    public Vector2 velocity = Vector2.zero;
    public Vector2 accelaration = Vector2.zero;
    public float angle = 0f;
    public float palstance = 0f;
    public float angularAccelaration = 0f;
    public float localTime = 0f;
    public ArrayList tempEvents;
    public ArrayList events = new ArrayList();
    public Actions.Move moveBy = (float t) => { return Vector2.zero; };
    public Actions.Move scaleWith = (float t) => { return Vector2.one; };
    public Actions.Rotate rotateBy = (float t) => { return 0; };
    public bool moveByDelegate = false;
    public bool scaleByDelegate = false;
    public bool rotateByDelegate = false;
    public bool tick = false;
    public bool bRetainTick = true;
    public bool operating = false;
    void Update()
    {
        if (operating) runAction();
    }
    public void runAction()
    {
        if (tick) localTime += Time.deltaTime;
        move(); turn(); scale();
        tempEvents = new ArrayList(events);
        foreach (Event evnt in tempEvents)
        {
            if (evnt.Key.delete()) { evnt.Key.debug(); evnt.Key.reset(); events.Remove(evnt); continue; }
            evnt.Key.updateTime();
            if (evnt.Key.excutable())
            {
                evnt.Value(this);
                evnt.Key.executedOnce();
            }
        }
    }
    private void updateTime()
    {
        localTime += Time.deltaTime;
    }
    private void move()
    {
        if (moveByDelegate)
        {
            velocity = moveBy(localTime);
        }
        else
        {
            velocity += accelaration * Time.deltaTime;
        }
        transform.localPosition = (Vector2)transform.localPosition + velocity * Time.deltaTime;
    }
    private void scale()
    {
        if (scaleByDelegate)
        {
            transform.localScale = scaleWith(localTime);
        }
    }
    private void turn()
    {
        if (rotateByDelegate)
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, rotateBy(localTime)));
        else
        {
            palstance += angularAccelaration * Time.deltaTime;
            angle += palstance;
        }
    }
    public void setPalstance(float newPals)
    {
        palstance = newPals;
    }
    public void setAcceleration(float acc)
    {
        accelaration = acc * velocity.normalized;
    }
    public void beginTick(float delay = 0f)
    {
        schedule(Actions.callFunc(() => { tick = true; }, delay));
    }
    public void pauseTick(float delay = 0f)
    {
        schedule(Actions.callFunc(() => { tick = false; }, delay));
    }
    public void resetTick(float delay = 0f)
    {
        schedule(Actions.callFunc(() => { localTime = 0; }, delay));
    }
    public void retainTick(float time, float delay = 0f)
    {
        schedule(Actions.callFunc(() => { bRetainTick = true; }, delay));
        schedule(Actions.callFunc(() => { bRetainTick = false; }, delay + time));
    }
    public float getRotation(float offSet = 0f)
    {
        return transform.rotation.eulerAngles.z - 90f + offSet;
    }
    public Vector2 getDirection(float offSet = 0f){
        float angleD = transform.rotation.eulerAngles.z - 90f + offSet;
        return new Vector2(Mathf.Cos(angleD * Mathf.Deg2Rad), Mathf.Sin(angleD * Mathf.Deg2Rad));
    }
    public void schedule(Event evnt)
    {
        events.Add(evnt);
    }
    public void setAwake(bool dir = true){
        tick = dir;
        operating = dir;
    }
}