using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FingerPrintLibrary
{
    public static class SensorFunctions
    {
        public static bool DetermineNextAvailablePosition(out short position, List<int> positions, int templateCapacity)
        {
            position = -1;

            if (positions.Count == 0)
            {
                position = 0;
                return true;
            }
            else if (positions.Count == 1)
            {
                if (positions[0] == 0)
                {
                    position = 1;
                }
                else
                {
                    position = 0;
                }
                return true;
            }
            else
            {
                if (positions[0] != 0)
                {
                    position = 0;
                    return true;
                }
                for (int i = 0; i < positions.Count - 1; i++)
                {
                    if (positions[i + 1] - positions[i] != 1)
                    {
                        position = (short)(i + 1);
                        if (position < templateCapacity)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                if (position == -1)
                {
                    position = (short)(positions[positions.Count - 1] + 1);
                    if (position >= templateCapacity)
                    {
                        position = -1;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
