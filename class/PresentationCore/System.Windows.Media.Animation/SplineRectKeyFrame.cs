
/* this file is generated by gen-animation-types.cs.  do not modify */

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;

namespace System.Windows.Media.Animation {


public class SplineRectKeyFrame : RectKeyFrame
{

	public static readonly DependencyProperty KeySplineProperty; // XXX initialize

	public SplineRectKeyFrame ()
	{
	}

	public SplineRectKeyFrame (Rect value)
	{
	}

	public SplineRectKeyFrame (Rect value, KeyTime keyTime)
	{
	}

	public SplineRectKeyFrame (Rect value, KeyTime keyTime, KeySpline keySpline)
	{
	}

	public KeySpline KeySpline {
		get { throw new NotImplementedException (); }
		set { throw new NotImplementedException (); }
	}

	protected override Freezable CreateInstanceCore ()
	{
		throw new NotImplementedException ();
	}

	protected override Rect InterpolateValueCore (Rect baseValue, double keyFrameProgress)
	{
		throw new NotImplementedException ();
	}
}


}
