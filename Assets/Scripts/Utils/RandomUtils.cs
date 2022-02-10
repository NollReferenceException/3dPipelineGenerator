using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomUtils
{
    static Vector3[] BasisVectors = new Vector3[]
    {
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back,
    };

    static int pastRandom = 0;


    public static Vector3 GetRandomBasisDirection()
    {
        int pointer = Random.Range(0, BasisVectors.Length);

        Vector3 direction;

        if (pointer == pastRandom)
        {
            direction = GetRandomBasisDirection();
        }
        else
        {
            direction = BasisVectors[pointer];
        }

        pastRandom = pointer;

        return direction;
    }
}
