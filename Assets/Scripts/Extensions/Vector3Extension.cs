using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extension
{
    public static Vector3 Deserialize(this Vector3 v3, PreSerializableVector4 serVector)
    {
        return new Vector3(serVector.a, serVector.b, serVector.c);
    }
}
