//
// DispatchOperation.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;

namespace System.ServiceModel.Dispatcher
{
	[MonoTODO]
	public sealed class DispatchOperation
	{
		internal class DispatchOperationCollection :
			SynchronizedKeyedCollection<string, DispatchOperation>
		{
			protected override string GetKeyForItem (DispatchOperation o)
			{
				return o.Name;
			}
		}

		DispatchRuntime parent;
		string name, action, reply_action;
		bool serialize_reply = true, deserialize_request = true,
			is_oneway, is_terminating,
			release_after_call, release_before_call,
			tx_auto_complete, tx_required;
		ImpersonationOption impersonation;
		IDispatchMessageFormatter formatter, actual_formatter;
		IOperationInvoker invoker;
		SynchronizedCollection<IParameterInspector> inspectors
			= new SynchronizedCollection<IParameterInspector> ();
		SynchronizedCollection<FaultContractInfo> fault_contract_infos
			= new SynchronizedCollection<FaultContractInfo> ();
		SynchronizedCollection<ICallContextInitializer> ctx_initializers
			= new SynchronizedCollection<ICallContextInitializer> ();
		OperationDescription description;

		public DispatchOperation (DispatchRuntime parent,
			string name, string action)
		{
			if (parent == null)
				throw new ArgumentNullException ("parent");
			if (name == null)
				throw new ArgumentNullException ("name");
			// action could be null

			is_oneway = true;
			this.parent = parent;
			this.name = name;
			this.action = action;
		}

		public DispatchOperation (DispatchRuntime parent,
			string name, string action, string replyAction)
			: this (parent, name, action)
		{
			// replyAction could be null
			is_oneway = false;
			reply_action = replyAction;
		}

		public string Action {
			get { return action; }
		}

		public SynchronizedCollection<ICallContextInitializer> CallContextInitializers {
			get { return ctx_initializers; }
		}

		public bool DeserializeRequest {
			get { return deserialize_request; }
			set { deserialize_request = value; }
		}

		public SynchronizedCollection<FaultContractInfo> FaultContractInfos {
			get { return fault_contract_infos; }
		}

		public IDispatchMessageFormatter Formatter {
			get { return formatter; }
			set {
				formatter = value;
				actual_formatter = null;
			}
		}

		public ImpersonationOption Impersonation {
			get { return impersonation; }
			set { impersonation = value; }
		}

		public IOperationInvoker Invoker {
			get { return invoker; }
			set { invoker = value; }
		}

		public bool IsOneWay {
			get { return is_oneway; }
		}

		public bool IsTerminating {
			get { return is_terminating; }
			set { is_terminating = value; }
		}

		public string Name {
			get { return name; }
		}

		public SynchronizedCollection<IParameterInspector> ParameterInspectors {
			get { return inspectors; }
		}

		public DispatchRuntime Parent {
			get { return parent; }
		}

		public bool ReleaseInstanceAfterCall {
			get { return release_after_call; }
			set { release_after_call = value; }
		}

		public bool ReleaseInstanceBeforeCall {
			get { return release_before_call; }
			set { release_before_call = value; }
		}

		public string ReplyAction {
			get { return reply_action; }
		}

		public bool SerializeReply {
			get { return serialize_reply; }
			set { serialize_reply = value; }
		}

		public bool TransactionAutoComplete {
			get { return tx_auto_complete; }
			set { tx_auto_complete = value; }
		}

		public bool TransactionRequired {
			get { return tx_required; }
			set { tx_required = value; }
		}

		MessageVersion MessageVersion {
			get { return Parent.ChannelDispatcher.MessageVersion; }
		}

		internal void ProcessRequest (RequestContext rc, OperationContext octx, TimeSpan sendTimeout)
		{
			try {
				DoProcessRequest (rc, octx, sendTimeout);
			} catch (Exception ex) {
				Message m = BuildExceptionMessage (rc.RequestMessage, ex);
				rc.Reply (m);
			}
		}

		private Message BuildExceptionMessage (Message req, Exception ex) {
			Console.WriteLine (ex);
			// FIXME: set correct name
			FaultCode fc = new FaultCode (
				"InternalServiceFault",
				req.Version.Addressing.Namespace);

	
			if (parent.ChannelDispatcher.IncludeExceptionDetailInFaults) {
				return Message.CreateMessage (req.Version, fc, ex.Message, new ExceptionDetail (ex), req.Headers.Action);
			}
			string faultString =
				@"The server was unable to process the request due to an internal error.  For more information about the error, either turn on IncludeExceptionDetailInFaults (either from ServiceBehaviorAttribute or from the &lt;serviceDebug&gt; configuration behavior) on the server in order to send the exception information back to the client, or turn on tracing as per the Microsoft .NET Framework 3.0 SDK documentation and inspect the server trace logs.";
			return Message.CreateMessage(req.Version, fc, faultString, req.Headers.Action);
		}

		void EnsureValid () {
			if (Invoker == null)
				throw new InvalidOperationException ("DispatchOperation requires Invoker.");
			if ((DeserializeRequest || SerializeReply) && Formatter == null)
				throw new InvalidOperationException ("The DispatchOperation '" + Name + "' requires Formatter, since DeserializeRequest and SerializeReply are not both false.");
		}

		void DoProcessRequest (RequestContext rc, OperationContext octx, TimeSpan sendTimeout)
		{
			Message req = rc.RequestMessage;
			object instance;
			object [] parameters;
			object [] ctx_initialization_results;
			BuildInvokeParams (req, out instance, out parameters, out ctx_initialization_results);

			if (Invoker.IsSynchronous) {
				object result = Invoker.Invoke (instance, parameters);
				HandleInvokeResult (rc, octx, sendTimeout, parameters, result, ctx_initialization_results);
			}
			else { // asynchronous
				Invoker.InvokeBegin (instance, parameters,
					delegate (IAsyncResult res) {
						object result;
						try {
							result = Invoker.InvokeEnd (instance, out parameters, res);
						}
						catch (Exception ex) {
							Message m = BuildExceptionMessage (req, ex);
							rc.Reply (m);
							return;
						}
						HandleInvokeResult (rc, octx, sendTimeout, parameters, result, ctx_initialization_results);
					},
					null);
			}
		}

		private void HandleInvokeResult (RequestContext rc, OperationContext octx, TimeSpan sendTimeout, object [] outputs, object result, object [] ctx_initialization_results) {
			for (int i = 0; i < ctx_initialization_results.Length; i++)
				CallContextInitializers [i].AfterInvoke (ctx_initialization_results [i]);

			Message res = null;
			if (SerializeReply)
				res = Formatter.SerializeReply (
					MessageVersion, outputs, result);
			else
				res = (Message) result;
			res.Headers.CopyHeadersFrom (octx.OutgoingMessageHeaders);
			res.Properties.CopyProperties (octx.OutgoingMessageProperties);
			rc.Reply (res, sendTimeout);
		}

		private void BuildInvokeParams (Message req, out object instance, out object [] parameters, out object [] ctx_initialization_results) {
			EnsureValid ();
			instance = null;
			if (parent.InstanceContextProvider != null) {
				InstanceContext ictx = parent.InstanceContextProvider.GetExistingInstanceContext (req, null);
				if (ictx == null)
					//FIXME: What should be done here?
					throw new InvalidOperationException ("The instance context provider failed to get or create an instance context");
				instance = ictx.GetServiceInstance ();
			}
			else {
				instance = parent.InstanceProvider != null ?
					parent.InstanceProvider.GetInstance (OperationContext.Current.InstanceContext, req) :
					// FIXME: this is hack to make simple things work.
					Activator.CreateInstance (Parent.ChannelDispatcher.Host.Description.ServiceType);
			}

			if (DeserializeRequest) {
				parameters = Invoker.AllocateParameters ();
				Formatter.DeserializeRequest (req, parameters);
			}
			else
				parameters = new object [] {req};

			ctx_initialization_results =   new object [CallContextInitializers.Count];

			for (int i = 0; i < ctx_initialization_results.Length; i++)
				// FIXME: get IClientChannel from somewhere.
				ctx_initialization_results [i] =
					CallContextInitializers [i].BeforeInvoke (OperationContext.Current.InstanceContext, null, req);
		}

		Message CreateActionNotSupported (Message req)
		{
			FaultCode fc = new FaultCode (
				req.Version.Addressing.ActionNotSupported,
				req.Version.Addressing.Namespace);
			// FIXME: set correct namespace URI
			return Message.CreateMessage (req.Version, fc,
				String.Format ("action '{0}' is not supported in this service contract.", req.Headers.Action), String.Empty);
		}

		internal void ProcessInput (Message req)
		{
			EnsureValid ();
			object instance = parent.InstanceProvider != null ?
				parent.InstanceProvider.GetInstance (OperationContext.Current.InstanceContext, req) : null;

			object [] parameters;
			if (DeserializeRequest) {
				// FIXME: invoker and formatter could be null.
				parameters = Invoker.AllocateParameters ();
				Formatter.DeserializeRequest (req, parameters);
			}
			else
				parameters = new object [] {req};

            if (Invoker.IsSynchronous)
				Invoker.Invoke (instance, parameters);
			else
				throw new NotImplementedException ();
		}
	}
}
