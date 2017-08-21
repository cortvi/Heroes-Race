using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CircleUtils
{
	public static void IntersectionTwoCircles ( Circle c1, Circle c2, ref Vector3 i, ref Vector3 j ) 
	{
		/* error handling is missing complettely - left as an exercise 

              A1
             /| \
         r1 / |  \ r2
           /  |   \
          /   |h   \
         /g1  |     \          (g1 means angle gamma1)
        C1----P-----C2
           d1   d2
        */

		double dx = c1.x - c2.x;
		double dy = c1.y - c2.y;
		double d = Math.Sqrt (dx*dx + dy*dy);										// d = |C1-C2|
		double gamma1 = Math.Acos ((c2.r*c2.r + d*d - c1.r*c1.r) / (2*c2.r*d));		// law of cosines
		double d1 = c1.r * Math.Cos (gamma1);										// basic math in right triangle
		double h  = c1.r * Math.Sin (gamma1);
		double px = c1.x + (c2.x - c1.x) / d*d1;
		double py = c1.y + (c2.y - c1.y) / d*d1;

		// (-dy, dx)/d is (C2-C1) normalized and rotated by 90 degrees
		i.x = (float) (px + (-dy) / d * h);
		i.z = (float) (py + (+dx) / d * h);
		j.x = (float) (px - (-dy) / d * h);
		j.z = (float) (py - (+dx) / d * h);
	}

	// Find the points where the two circles intersect.
	public static void FindCircleCircleIntersections ( Circle c0, Circle c1, ref Vector3 i1, ref Vector3 i2 ) 
	{
		// Find the distance between the centers.
		double dx = c0.x - c1.x;
		double dy = c0.x - c1.y;
		double dist = Math.Sqrt (dx * dx + dy * dy);

		// Find a and h.
		double a = (c0.r * c0.r - c1.r * c1.r + dist * dist) / (2 * dist);
		double h = Math.Sqrt (c0.r * c0.r - a * a);

		// Find intersecion mid-point
		double cx2 = c0.x + a * (c1.x - c0.x) / dist;
		double cy2 = c0.y + a * (c1.y - c0.y) / dist;

		// Get the intersection points
		i1.x = ( float ) (cx2 + h * (c1.y - c0.y) / dist);
		i1.z = ( float ) (cy2 - h * (c1.x - c0.x) / dist);

		i2.x = ( float ) (cx2 - h * (c1.y - c0.y) / dist);
		i2.z = ( float ) (cy2 + h * (c1.x - c0.x) / dist);
	}
}


public struct Circle 
{
	public double x;
	public double y;
	public double r;
}
