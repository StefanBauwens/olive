
/* this file is generated by gen-animation-types.cs.  do not modify */

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;

namespace System.Windows.Media.Animation {


public class SplineInt64KeyFrame : Int64KeyFrame
{

	public static readonly DependencyProperty KeySplineProperty =
		DependencyProperty.Register ("KeySpline", typeof (KeySpline), typeof (SplineInt64KeyFrame));

	long value;
	KeyTime keyTime;

	public SplineInt64KeyFrame ()
	{
	}

	public SplineInt64KeyFrame (long value)
	{
		this.value = value;
		// XX keytime?
	}

	public SplineInt64KeyFrame (long value, KeyTime keyTime)
	{
		this.value = value;
		this.keyTime = keyTime;
	}

	public SplineInt64KeyFrame (long value, KeyTime keyTime, KeySpline keySpline)
	{
		this.value = value;
		this.keyTime = keyTime;
		KeySpline = keySpline;
	}

	public KeySpline KeySpline {
		get { return (KeySpline)GetValue (KeySplineProperty); }
		set { SetValue (KeySplineProperty, value); }
	}

	protected override Freezable CreateInstanceCore ()
	{
		return new SplineInt64KeyFrame ();
	}

	protected override long InterpolateValueCore (long baseValue, double keyFrameProgress)
	{
		double splineProgress = KeySpline.GetSplineProgress (keyFrameProgress);

		return (long)(baseValue + (value - baseValue) * splineProgress);
	}
}


}
