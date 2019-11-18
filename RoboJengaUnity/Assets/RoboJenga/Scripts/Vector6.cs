using System;


public struct Vector6
{
    public float A1;
    public float A2;
    public float A3;
    public float A4;
    public float A5;
    public float A6;

    public Vector6(float a1, float a2, float a3, float a4, float a5, float a6)
    {
        A1 = a1;
        A2 = a2;
        A3 = a3;
        A4 = a4;
        A5 = a5;
        A6 = a6;
    }

    public Vector6(float[] joints)
    {
        A1 = joints[0];
        A2 = joints[1];
        A3 = joints[2];
        A4 = joints[3];
        A5 = joints[4];
        A6 = joints[5];
    }

    public float this[int i]
    {
        get
        {
            switch (i)
            {
                case 0: return A1;
                case 1: return A2;
                case 2: return A3;
                case 3: return A4;
                case 4: return A5;
                case 5: return A6;
                default:
                    throw new IndexOutOfRangeException("Invalid Vector6 index.");
            }
        }

        set
        {
            switch (i)
            {
                case 0: A1 = value; break;
                case 1: A2 = value; break;
                case 2: A3 = value; break;
                case 3: A4 = value; break;
                case 4: A5 = value; break;
                case 5: A6 = value; break;
                default:
                    throw new IndexOutOfRangeException("Invalid Vector6 index.");
            }
        }
    }
}
