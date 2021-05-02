using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Event = System.Collections.Generic.KeyValuePair<Condition, Actions.Func>;

public class Actions : MonoBehaviour
{
    public delegate void Action();
    public delegate void Func(Node node);
    public delegate bool Exit();
    public delegate Vector2 Move(float t);
    public delegate float Rotate(float t);

    public ArrayList events = new ArrayList();

    public static Event callFunc(Action action, float delay = 0f, int times = 1)
    {
        Condition cond = new Condition(delay, times);
        Func func = (Node node) => { action(); };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event delay(float time)
    {
        Condition cond = new Condition(time, 1);
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
    public static Event setVelocity(Vector2 velocity, float delay = 0f)
    {
        Condition cond = new Condition(delay, 1);
        Func func = (Node node) =>
        { node.velocity = velocity; node.moveByDelegate = false; };
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
    public static Event setScale(Move move, float delay = 0f)
    {
        Condition cond = new Condition(delay, 1);
        Func func = (Node node) =>
        { node.scaleByDelegate = true; node.scaleWith = move; };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event scaleTo(Vector2 scale, float time, float delay = 0f)
    {
        Condition cond = new Condition(0f, 1, delay);
        Func func = (Node node) =>
        {
            node.scaleByDelegate = true;
            Vector2 initScale = node.transform.localScale;
            Vector2 scaleuni = (scale - (Vector2)node.transform.localScale) / time;
            node.scaleWith = (float t) => { return initScale + scaleuni * t; };
            node.schedule(Actions.callFunc(() => { node.moveByDelegate = false; }, time));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event tweenBy(Vector2 dis, float time, float delay = 0f)
    {
        Condition cond = new Condition(0f, 1, delay);
        Func func = (Node node) =>
        {
            Vector2 pos = node.transform.localPosition;
            Vector2 velocity = dis / time;
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
            node.schedule(evnt: callFunc(() => { node.moveBy = (float t) => { return Vector2.zero; };
            node.transform.localPosition = pos + dis; }, time));
            //node.schedule(stop(time));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event tweenTo(Vector2 pos, float time, float delay = 0f)
    {
        Condition cond = new Condition(0f, 1, delay);
        Func func = (Node node) =>
        {
            Vector2 velocity = (pos - (Vector2)node.transform.position) / time;
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
            node.schedule(callFunc(() => { node.transform.localPosition = pos; }, time));
            node.schedule(Actions.stop(time));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event moveTo(Vector2 pos, float time, bool bStop = true, float delay = 0f)
    {
        Condition cond = new Condition(0f, 1, delay);
        Func func = (Node node) =>
        {
            node.moveByDelegate = false;
            Vector2 velocity = (pos - (Vector2)node.transform.position) / time;
            node.velocity = velocity;
            if (bStop) node.schedule(stop(time));
        };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event setAccelaration(Vector2 accelaration, float delay = 0f)
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
        { node.velocity = Vector2.zero; node.accelaration = Vector2.zero; node.moveByDelegate = false; };
        Event evnt = new KeyValuePair<Condition, Func>(cond, func);
        return evnt;
    }
    public static Event repeatForever(params Event[] events)
    {
        float time = 0f;
        float time2 = 0f;
        for (int i = 0; i < events.Length; i++)
        {
            time2 = time;
            time2 += events[i].Key.timeUpperBound * events[i].Key.period;
            events[i].Key.period += time;
            time = time2;
        }
        Condition cond = new Condition(time, 0);
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
        float time2 = 0f;
        for (int i = 0; i < events.Length; i++)
        {
            time2 = time;
            time2 += events[i].Key.timeUpperBound * events[i].Key.period;
            events[i].Key.period += time;
            time = time2;
        }
        Condition cond = new Condition(time, 1);
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
}

