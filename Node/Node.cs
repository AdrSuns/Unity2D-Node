using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Event = System.Collections.Generic.KeyValuePair<Condition, Actions.Func>;

public class Node : MonoBehaviour
{
    public static float deltaTime = 0f;
    public static float timeScale = 0f;
    public static bool timeUptoDate = false;
    public Vector3 velocity = Vector3.zero;
    public Vector3 accelaration = Vector3.zero;
    public bool checkRotate = false;
    public float checkRotateOffSet = 0f;
    public float angle = 0f;
    public float palstance = 0f;
    public float angularAccelaration = 0f;
    public float localTime = 0f; 
    public List<Event> events = new List<Event>();
    public Actions.Move moveBy = (float t) => { return Vector3.zero; };
    public Actions.Scale scaleWith = (float t) => { return Vector3.one; };
    public Actions.Rotate rotateBy = (float t) => { return 0; };
    public bool isRigidBody;
    public Rigidbody2D nodeRb;
    public bool moveByDelegate = false;
    public bool scaleByDelegate = false;
    public bool rotateByDelegate = false;
    public bool moveByNode = true;
    public bool rotateByNode = true;
    public bool tick = false;
    public bool useRealTime = false;
    public bool bRetainTick = true;
    public bool operating = false;
    private void Awake() {
        angle = transform.localEulerAngles.z;
        nodeRb = GetComponent<Rigidbody2D>();
        isRigidBody = nodeRb != null;
    }
    public void nodeUpdate()
    {
        if (operating) runAction();
    }
    void Update()
    {
        if (operating) runAction();
    }
    public void runAction()
    {
        if (tick) { updateTime(); }
        checkEvent();
        if (timeScale != 0f || useRealTime){
            if (moveByNode) move();
            if (rotateByNode) turn();
            scale();
        }
    }
    public void checkEvent(){
        for (int i = 0; i < events.Count; i++)
        {
            Event evnt = events[i];
            if (evnt.Key.delete()) { evnt.Key.reset(); events.Remove(evnt); i--; continue; }
            evnt.Key.updateTime(useRealTime);
            if (evnt.Key.excutable())
            {
                evnt.Value(this);
                evnt.Key.executedOnce();
            }
        }
        /*
        List<Event> tempEvents = new List<Event>(events);
        foreach (Event evnt in tempEvents)
        {
            if (evnt.Key.delete()) { evnt.Key.reset(); events.Remove(evnt); continue; }
            evnt.Key.updateTime(useRealTime);
            if (evnt.Key.excutable())
            {
                evnt.Value(this);
                evnt.Key.executedOnce();
            }
        }
        */
    }
    public void updateTime()
    {
        if (!useRealTime)
        {
            if (!timeUptoDate) {
                deltaTime = Time.deltaTime;
                timeScale = Time.timeScale;
            }
            localTime += deltaTime * timeScale;
        }
        else localTime += GameControl.instance.realDeltaTime;
    }
    public void move()
    {
        if (moveByDelegate)
        {
            velocity = moveBy(localTime);
        }
        else
        {
            if (accelaration != Vector3.zero)
                velocity += accelaration * ((useRealTime) ? GameControl.instance.realDeltaTime : deltaTime);
            if (velocity == Vector3.zero) return;
        }
        if (checkRotate)
        {
            if (velocity.x != 0)
            {
                angle = Actions.getRotation(velocity) + checkRotateOffSet;
            }
        }
        //if (isRigidBody) nodeRb.velocity = velocity;//nodeRb.position += (Vector2)velocity * (useRealTime ? GameControl.instance.realDeltaTime : deltaTime);
        //else
        transform.localPosition = transform.localPosition + velocity * (useRealTime ? GameControl.instance.realDeltaTime : deltaTime);
    }
    public void scale()
    {
        if (scaleByDelegate)
        {
            transform.localScale = scaleWith(localTime);
        }
    }
    public void turn()
    {
        if (rotateByDelegate)
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, rotateBy(localTime)));
        else
        {
            if (angularAccelaration != 0) palstance += angularAccelaration * deltaTime;
            if (palstance != 0) angle += palstance;
        }
        /*
        if (isRigidBody) nodeRb.MoveRotation(angle);
        else */
        if (transform.localEulerAngles.z != angle) transform.localEulerAngles = Vector3.forward * angle;
    }
    public void setPalstance(float newPals)
    {
        palstance = newPals;
    }
    public void setVelocity(Vector3 vel, float delay = 0f)
    {
        if (delay == 0f)
        {
            moveByDelegate = false;
            velocity = vel;
        }
        else schedule(Actions.callFunc(() =>
        {
            moveByDelegate = false;
            velocity = vel;
        }, delay));
    }
    public void addVelocity(Vector3 vel, float delay = 0f)
    {
        if (delay == 0f)
        {
            moveByDelegate = false;
            velocity += vel;
        }
        else schedule(Actions.callFunc(() =>
        {
            moveByDelegate = false;
            velocity += vel;
        }, delay));
    }
    public void setAcceleration(float acc, float delay = 0f, float angleOffset = 0f)
    {
        if (acc == 0f) setAcceleration(Vector3.zero, delay);
        else if (angleOffset != 0){
          setAcceleration(acc * Actions.getDirection(angle + angleOffset), delay);
        } else setAcceleration(acc * velocity.normalized, delay);
    }
    public void setAcceleration(Vector3 acc, float delay = 0f)
    {
        if (delay == 0f){
            moveByDelegate = false;
            accelaration = acc;
        }
        else schedule(Actions.callFunc(() => {
            moveByDelegate = false;
            accelaration = acc;
        }, delay));
    }
    public void setOpacity(float opc){
        SpriteRenderer rend = GetComponent<SpriteRenderer>();
        rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, opc);
    }
    public void beginTick(float delay = 0f)
    {
        if (delay == 0f) tick = true;
        else schedule(Actions.callFunc(() => { tick = true; }, delay));
    }
    public void pauseTick(float delay = 0f, bool keepOperating = true)
    {
        schedule(Actions.callFunc(() => { tick = false; if (!keepOperating) operating = false; }, delay));
    }
    public void resetTick(float delay = 0f)
    {
        schedule(Actions.callFunc(() => { localTime = 0; }, delay));
    }
    public void tempTick(float time, bool keepOperating = true){
        setAwake();
        pauseTick(time, keepOperating);
        resetTick(time);
    }
    public void retainTick(float time, float delay = 0f)
    {
        schedule(Actions.callFunc(() => { bRetainTick = true; }, delay));
        schedule(Actions.callFunc(() => { bRetainTick = false; }, delay + time));
    }
    public void realTimeMode(bool dir = true){
        useRealTime = dir;
    }
    public float getRotation(float offSet = 0f)
    {
        return transform.rotation.eulerAngles.z - 90f + offSet;
    }
    public Vector3 getDirection(float offSet = 0f){
        float angleD = transform.rotation.eulerAngles.z - 90f + offSet;
        return new Vector3(Mathf.Cos(angleD * Mathf.Deg2Rad), Mathf.Sin(angleD * Mathf.Deg2Rad));
    }
    public float getPlayerDirection()
    {
        Vector3 vector = (Vector3)(Player.instance.transform.localPosition - transform.position).normalized;
        return Mathf.Atan(vector.y / vector.x) * Mathf.Rad2Deg + ((transform.position.x <= Player.instance.transform.localPosition.x) ? 0f : 180f); ;
    }
    public void setCheckRotate(float offSet = 0f){
        checkRotate = true;
        checkRotateOffSet = offSet;
    }
    public void schedule(Event evnt)
    {
        events.Add(evnt);
    }
    public void setAwake(bool dir = true){
        tick = dir;
        operating = dir;
    }
    public void removeSelf(float delay = 0f){
        if (delay == 0f) Destroy(gameObject);
        else schedule(Actions.callFunc(() => { Destroy(gameObject); }, delay));
    }
}