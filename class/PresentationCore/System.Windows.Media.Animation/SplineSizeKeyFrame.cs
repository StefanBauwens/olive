
/* this file is generated by gen-animation-types.cs.  do not modify */

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;

namespace System.Windows.Media.Animation {


public class SplineSizeKeyFrame : SizeKeyFrame
{

	public static readonly DependencyProperty KeySplineProperty; // XXX initialize

	public SplineSizeKeyFrame ()
	{
	}

	public SplineSizeKeyFrame (Size value)
	{
	}

	public SplineSizeKeyFrame (Size value, KeyTime keyTime)
	{
	}

	public SplineSizeKeyFrame (Size value, KeyTime keyTime, KeySpline keySpline)
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

	protected override Size InterpolateValueCore (Size baseValue, double keyFrameProgress)
	{
		throw new NotImplementedException ();
	}
}


}
