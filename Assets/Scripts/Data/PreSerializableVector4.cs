using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[Serializable]
public class PreSerializableVector4
{
    public float a;
    public float b;
    public float c;
    public float d;

    public Vector3 ToVector3()
    {
        return new Vector3(a, b, c);
    }

    public static implicit operator Quaternion(PreSerializableVector4 sv)
    {
        return new Quaternion(sv.a, sv.b, sv.c, sv.d);
    }

    public static implicit operator PreSerializableVector4(Quaternion q)
    {
        return new PreSerializableVector4()
        {
            a = q.x,
            b = q.y,
            c = q.z,
            d = q.w
        };
    }

    public static implicit operator Color(PreSerializableVector4 sv)
    {
        return new Color(sv.a, sv.b, sv.c, sv.d);
    }

    public static implicit operator PreSerializableVector4(Color c)
    {
        return new PreSerializableVector4()
        {
            a = c.r,
            b = c.g,
            c = c.b,
            d = c.a
        };
    }

    public static implicit operator Vector2(PreSerializableVector4 sv)
    {
        return new Vector2(sv.a, sv.b);
    }

    public static implicit operator PreSerializableVector4(Vector2 v)
    {
        return new PreSerializableVector4()
        {
            a = v.x,
            b = v.y
        };
    }

    public static implicit operator PreSerializableVector4(Vector3 v)
    {
        return new PreSerializableVector4()
        {
            a = v.x,
            b = v.y,
            c = v.z
        };
    }
}

