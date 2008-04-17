﻿//
// HttpChannelManager.cs
//
// Author:
//	Vladimir Krasnov <vladimirk@mainsoft.com>
//
// Copyright (C) 2005-2006 Mainsoft, Inc.  http://www.mainsoft.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace System.ServiceModel.Channels
{
	internal class HttpChannelManager<TChannel> where TChannel : class, IChannel
	{
		static Dictionary<Uri, HttpListener> opened_listeners;
		HttpChannelListener<TChannel> channel_listener;
		HttpListener http_listener;

		static HttpChannelManager ()
		{
			opened_listeners = new Dictionary<Uri, HttpListener> ();
		}

		public HttpChannelManager (HttpChannelListener<TChannel> channel_listener)
		{
			this.channel_listener = channel_listener;
		}

		public void Open (TimeSpan timeout)
		{
			if (opened_listeners.ContainsKey (channel_listener.Uri))
				http_listener = opened_listeners [channel_listener.Uri];

			if (http_listener == null) {
				http_listener = new HttpListener ();

				string uriString = channel_listener.Uri.ToString ();
				if (!uriString.EndsWith ("/", StringComparison.Ordinal))
					uriString += "/";
				http_listener.Prefixes.Add (uriString);
				http_listener.Start ();

				opened_listeners [channel_listener.Uri] = http_listener;
			}
		}

		public void Stop ()
		{
			if (http_listener == null)
				return;
			if (http_listener.IsListening)
				http_listener.Stop ();
			((IDisposable) http_listener).Dispose ();
		}

		public HttpListener HttpListener
		{
			get { return http_listener; }
		}
	}
}
