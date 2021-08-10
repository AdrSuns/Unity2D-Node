using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Event = System.Collections.Generic.KeyValuePair<Condition, Actions.Func>;

public class Actions : MonoBehaviour
{
    public delegate void Action();
    public delegate void Func(Node node);
    public delegate bool Exit();
    public delegate Vector3 Move(float t);
    public delegate Vector3 Scale(float t);
    public delegate float Rotate(float t);

    public ArrayList events = new ArrayList();

    public static Event callFunc(Action action, float delay = 0f, int times = 1, float offSet = 0f)
    {
        Condition cond = new Condition(delay, times, offSet);
        Func func = (Node node) => { action(); };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }

    public static Event callFunc(Func func, float delay = 0f, int times = 1, float offSet = 0f)
    {
        Condition cond = new Condition(delay, times, offSet);
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event delay(float time)
    {
        Condition cond = new Condition(time, 1, time);
        Func func = (Node node) => { };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event setVelocity(Move move, bool resetTick = false, float delay = 0f)
    {
        Condition cond = new Condition(delay, 1);
        Func func = (Node node) =>
        { node.moveBy = move; node.moveByDelegate = true; if (resetTick) node.resetTick(); };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event setVelocity(Vector3 velocity, float delay = 0f)
    {
        Condition cond = new Condition(delay, 1);
        Func func = (Node node) =>
        { node.velocity = velocity; node.moveByDelegate = false; };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event addVelocity(Move move, bool resetTick = false, float delay = 0f)
    {
        Condition cond = new Condition(delay, 1);
        Func func = (Node node) =>
        { node.moveBy += move; node.moveByDelegate = true; if (resetTick) node.resetTick(); };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event setRotation(Rotate rotate, float delay = 0f)
    {
        Condition cond = new Condition(delay, 1);
        Func func = (Node node) =>
        { node.rotateByDelegate = true; node.rotateBy = rotate; };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event setScale(Scale move, float delay = 0f)
    {
        Condition cond = new Condition(delay, 1);
        Func func = (Node node) =>
        { node.scaleByDelegate = true; node.scaleWith = move; };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event scaleTo(Vector3 scale, float time, float delay = 0f, bool turnOff = false)
    {
        Condition cond = new Condition(time, 1, time - delay);
        Func func = (Node node) =>
        {
            node.scaleByDelegate = true;
            float oriTime = node.localTime;
            Vector3 initScale = node.transform.localScale;
            Vector3 scaleuni = (scale - node.transform.localScale) / time;
            node.scaleWith = (float t) => { return (t - oriTime < time) ? initScale + scaleuni * (t - oriTime) : scale; };
            if (turnOff) node.schedule(Actions.callFunc(() => { node.scaleByDelegate = false; }, time));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event fadeOut(float time, float delay = 0f)
    {
        Condition cond = new Condition(time, 1, time - delay);
        Func func = (Node node) =>
        {
            SpriteRenderer rend = node.GetComponent<SpriteRenderer>();
            float uni = 1 / time;
            Actions.Action fade = () => { 
                Color col = rend.color;
                col.a -= uni * ((node.useRealTime) ? GameControl.instance.realDeltaTime : Time.deltaTime);
                rend.color = col;
            };
            node.schedule(repeatUntil(fade, 0f, () => { return node.GetComponent<SpriteRenderer>().color.a <= 0; }));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event fadeTo(float opc, float time, float delay = 0f, Image img = null)
    {
        Condition cond = new Condition(0f, 1, delay);
        Func func = (Node node) =>
        {
            SpriteRenderer rend = node.GetComponent<SpriteRenderer>();
            if (time == 0f) { node.setOpacity(opc); }
            float dir = (rend.color.a > opc) ? -1f : 1f;
            float uni = (opc - rend.color.a) / time;
            Actions.Action fade = () =>
            {
                Color col = rend.color;
                col.a += uni * ((node.useRealTime) ? GameControl.instance.realDeltaTime : Time.deltaTime);
                if (dir * col.a >= dir * opc) col.a = opc;
                rend.color = col;
            };
            node.schedule(repeatUntil(fade, 0f, () => { return node.GetComponent<SpriteRenderer>().color.a == opc; }));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event imageFadeTo(float opc, float time, float delay = 0f)
    {
        Condition cond = new Condition(0f, 1, delay);
        Func func = (Node node) =>
        {
            Image rend = node.GetComponent<Image>();
            if (time == 0f)
            {
                rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, opc);
                return;
            }
            float dir = (rend.color.a > opc) ? -1f : 1f;
            float uni = (opc - rend.color.a) / time;
            Actions.Action fade = () =>
            {
                Color col = rend.color;
                col.a += uni * ((node.useRealTime) ? GameControl.instance.realDeltaTime : Time.deltaTime);
                if (dir * col.a >= dir * opc) col.a = opc;
                rend.color = col;
            };
            node.schedule(repeatUntil(fade, 0f, () => { return node.GetComponent<Image>().color.a == opc; }));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event removeSelf(float delay = 0f)
    {
        Condition cond = new Condition(delay, 1, 0f);
        Func func = (Node node) =>
        {
            node.removeSelf();
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event shake(Vector3 dis, float time, float delay = 0f, bool additive = false)
    {
        //Condition cond = new Condition(0f, 1, delay);
        Condition cond = new Condition(delay, 1, 0f);
        Func func = (Node node) =>
        {
            Vector3 pos = node.transform.localPosition;
            Vector3 velocity = 4f * dis / time;
            float timeOs = node.localTime;
            node.moveByDelegate = true;
            node.moveBy = (float tl) =>
            {
                float t = tl - timeOs;
                return 4f * dis / time * (1 - 2f / time * t);
            };
            node.schedule(evnt: callFunc(() =>
            {
                node.moveBy = (float t) => { return Vector3.zero; };
                if (!additive) node.transform.localPosition = pos;
            }, time));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event chasePlayer(float speed, float delay = 0f, float phase = -1f, bool remove = true)
    {
        Condition cond = new Condition(0f, 1, 0f);
        Func func = (Node node) =>
        {
            node.schedule(repeatUntil(() => {
                Vector3 distance = Player.instance.transform.localPosition - node.transform.position;
                Vector3 vector = distance.normalized;
                if (remove && getLength(distance) < Time.deltaTime * speed) Destroy(node.gameObject);
                node.transform.localPosition += vector * Time.deltaTime * speed;
            }, delay, () => { return phase != -1f && node.localTime >= phase; }));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event tweenBy(Vector3 dis, float time, float delay = 0f, bool helpPos = true, bool local = true)
    {
        Condition cond = new Condition(time, 1, time - delay);
        Func func = (Node node) =>
        {
            Vector3 pos = local ? node.transform.localPosition : node.transform.position;
            Vector3 velocity = dis / time;
            float timeOs = node.localTime;
            node.moveByDelegate = true;
            node.moveBy = (float tl) =>
            {
                float t = tl - timeOs;
                if (t < time * 0.5f)
                {
                    return (4f * t / time) * velocity;
                }
                else return (4f * (time - t) / time) * velocity;
            };
            node.schedule(evnt: callFunc(() =>
            {
                node.moveBy = (float t) => { return Vector3.zero; };
                if (helpPos)
                {
                    if (node.isRigidBody) node.nodeRb.position = pos + dis;
                    else node.transform.localPosition = pos + dis;
                }
            }, time));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event tweenTo(Vector3 pos, float time, float delay = 0f, bool helpPos = true, bool local = true)
    {
        Condition cond = new Condition(time, 1, time - delay);
        Func func = (Node node) =>
        {
            Vector3 velocity = (pos - (local ? node.transform.localPosition : node.transform.position)) / time;
            float timeOs = node.localTime;
            node.moveByDelegate = true;
            Transform nodeRef = node.transform;
            node.moveBy = (float tl) =>
            {
                float t = tl - timeOs;
                if (t < time * 0.5f)
                {
                    return (4f * t / time) * velocity;
                }
                else return (4f * (time - t) / time) * velocity;
            };
            if (helpPos) node.schedule(callFunc(() => { {
                    if (node.isRigidBody) node.nodeRb.position = pos;
                    else node.transform.localPosition = pos;
                }}, time));
            node.schedule(Actions.stop(time));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }public static Event chase(GameObject obj, float speed, float delay = 0f, float phase = -1f, bool remove = true)
    {
        Condition cond = new Condition(0f, 1, 0f);
        Func func = (Node node) =>
        {
            node.moveByDelegate = false;

            node.schedule(repeatUntil(() =>
            {
                Vector3 distance = obj.transform.position - node.transform.position;
                Vector3 vector = distance.normalized;
                if (remove && getLength(distance) < Time.deltaTime * speed) Destroy(node.gameObject);
                node.velocity = vector * speed;
            }, delay, () => { return phase != -1f && node.localTime >= phase; }));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event moveTo(Vector3 pos, float time, bool bStop = true, float delay = 0f, bool helpPos = false)
    {
        Condition cond = new Condition(0f, 1, delay);
        Func func = (Node node) =>
        {
            node.moveByDelegate = false;
            Vector3 velocity = (pos - node.transform.localPosition) / time;
            node.velocity = velocity;
            if (bStop) node.schedule(stop(time));
            if (helpPos) node.schedule(callFunc(() =>
            {
                node.transform.localPosition = pos;
            }, time));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event dashBy(Vector3 pos, float time, float delay = 0f)
    {
        Condition cond = new Condition(time, 1, time - delay);
        Func func = (Node node) =>
        {
            node.moveByDelegate = true;
            node.velocity = Vector3.zero;
            Vector3 oriPos = node.transform.localPosition;
            float oriTime = node.localTime;
            node.moveBy = (float t) => { return 2f * pos / time * (node.localTime - oriTime) / time; };
            node.schedule(Actions.callFunc(() => { node.transform.localPosition = oriPos + pos; }, time));
            node.schedule(stop(time));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event setAccelaration(Vector3 accelaration, float delay = 0f)
    {
        Condition cond = new Condition(delay, 1);
        Func func = (Node node) =>
        { node.accelaration = accelaration; node.moveByDelegate = false; };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event stop(float delay = 0f)
    {
        Condition cond = new Condition(delay, 1);
        Func func = (Node node) =>
        { node.velocity = Vector3.zero; node.accelaration = Vector3.zero; node.moveByDelegate = false; };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event repeatForever(params Event[] events)
    {
        float time = 0f;
        float period = 0f;
        for (int i = 0; i < events.Length; i++) period += events[i].Key.timeUpperBound * events[i].Key.period;
        for (int i = 0; i < events.Length; i++)
        {
            events[i].Key.localTime = events[i].Key.period - events[i].Key.localTime + period - time;
            time += events[i].Key.timeUpperBound * events[i].Key.period;
            events[i].Key.period = period;
        }
        Condition cond = new Condition(time, 0, time);
        Func func = (Node node) =>
        {
            for (int i = 0; i < events.Length; i++) node.schedule(events[i]);
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event repeatForever(Action action, float delay, float offSet = 0f)
    {
        Condition cond = new Condition(delay, 0, offSet);
        Func func = (Node node) =>
        {
            action();
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event repeatUntil(Action action, float delay, Exit exit, float offSet = 0f)
    {
        Condition cond = new Condition(delay, 0, offSet);
        cond.setExitFunc(exit);
        Func func = (Node node) =>
        {
            action();
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event repeatUntil(Event evnt0, Exit exit, float offSet = 0f)
    {
        Condition cond = new Condition(evnt0.Key.period, 0, offSet);
        cond.setExitFunc(exit);
        Func func = (Node node) =>
        {
            evnt0.Value(node);
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event executeOn(Event evnt0, Exit start, float delay = 0f)
    {
        Condition cond = new Condition(0f, 0, delay);
        Exit exit = () => { return !start(); };
        Func func = (Node node) =>
        {
            if (start()) { evnt0.Value(node); cond.setExitFunc(() => { return true; }); }
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event sequence(params Event[] events)
    {
        float time = 0f;
        float period = 0f;
        for (int i = 0; i < events.Length; i++) period += events[i].Key.timeUpperBound * events[i].Key.period;
        for (int i = 0; i < events.Length; i++){
            events[i].Key.localTime = events[i].Key.period - events[i].Key.localTime + period - time;
            time += events[i].Key.timeUpperBound * events[i].Key.period;
            events[i].Key.period = period;
        }
        Condition cond = new Condition(time, 1, time);
        Func func = (Node node) =>
        {
            for (int i = 0; i < events.Length; i++) node.schedule(events[i]);
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event spawn(params Event[] events)
    {
        float time = 0f;
        for (int i = 0; i < events.Length; i++)
        {
            if (time < events[i].Key.period)
                time = events[i].Key.period;
        }
        Condition cond = new Condition(time, 1, time);
        Func func = (Node node) =>
        {
            for (int i = 0; i < events.Length; i++) node.schedule(events[i]);
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Vector3 getDirection(float angle){
        return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
    }
    public static float getRotation(Vector3 vec)
    {
        if (vec.x != 0)
        {
            return Mathf.Rad2Deg * Mathf.Atan((vec.y * 100f) / (vec.x * 100f)) - 90f + (vec.x > 0 ? 0f : 180f);
        }
        else return vec.y >= 0 ? 90f : -90f;
    }
    public static void rotate(ref Vector3 vec, float angle)
    {
        Vector3 ori = vec;
        float length = getLength(ori);
        vec.x = ori.x * Mathf.Cos(angle * Mathf.Deg2Rad) - ori.y * Mathf.Sin(angle * Mathf.Deg2Rad);
        vec.y = ori.x * Mathf.Sin(angle * Mathf.Deg2Rad) + ori.y * Mathf.Cos(angle * Mathf.Deg2Rad);
    }
    public static void flip(ref Vector3 vec, float angle){
        float delAng = angle - getRotation(vec);
        rotate(ref vec, delAng * 2f);
    }
    public static float getLength(Vector3 vec)
    {
        return Mathf.Pow(vec.x * vec.x + vec.y * vec.y, 0.5f);
    }
}

