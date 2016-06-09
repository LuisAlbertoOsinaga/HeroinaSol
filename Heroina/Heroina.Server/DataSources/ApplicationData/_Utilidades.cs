using System;

namespace LightSwitchApplication
{
    public static class Utilidades
    {
        public static bool IsTimeStampEqual(byte[] timestamp1, byte[] timestamp2)
        {
            if (timestamp1 == null || timestamp2 == null)
                return false;

            if (timestamp1.Length != timestamp2.Length)
                return false;

            for (int i = 0; i < timestamp1.Length; i++)
            {
                if (timestamp1[i] != timestamp2[i])
                    return false;
            }

            return true;
        }
    }
}
