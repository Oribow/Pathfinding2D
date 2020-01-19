using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavLinePostProcessor
{
    void Process(List<NavLine> navLines, NavAgentType navAgentType, float floatToIntMult)
    {

        //1. convert coordinates back
        ConvertCoordinates(navLines, floatToIntMult);

        foreach (var line in navLines)
        {
            float totalLength = 0;

            var prevSeg = line.segments[0];
            for (int iSeg = 1; iSeg < line.segments.Length; iSeg++)
            {
                Vector2 dir = line.segments[iSeg].start - prevSeg.start;
                totalLength += dir.magnitude;

                prevSeg = line.segments[iSeg];
            }

            if (totalLength < navAgentType.width)
            {
                // line is smaller than width
            }
        }
    }

    private void ConvertCoordinates(List<NavLine> navLines, float floatToIntMult)
    {
        foreach (var line in navLines)
        {
            foreach (var segment in line.segments)
            {
                segment.start /= floatToIntMult;
            }
        }
    }
}
