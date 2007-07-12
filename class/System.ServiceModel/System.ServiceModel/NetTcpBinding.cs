//
// NetTcpBinding.cs
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
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Xml;

namespace System.ServiceModel
{
	[MonoTODO]
	public class NetTcpBinding : Binding, IBindingRuntimePreferences
	{
		HostNameComparisonMode comparison_mode;
		int listen_backlog;
		long max_pool_size;
		int max_buf_size;
		int max_conn;
		long max_msg_size;
		OptionalReliableSession reliable_session;
		NetTcpSecurity security;
		bool port_sharing_enabled;
		XmlDictionaryReaderQuotas reader_quotas;
		EnvelopeVersion soap_version;
		bool transaction_flow;
		TransactionProtocol transaction_protocol;
		TransferMode transfer_mode;

		public NetTcpBinding ()
			: this (SecurityMode.Message)
		{
		}

		public NetTcpBinding (SecurityMode securityMode)
			: this (securityMode, false)
		{
		}

		public NetTcpBinding (SecurityMode securityMode,
			bool reliableSessionEnabled)
		{
			security = new NetTcpSecurity (securityMode);
		}

		public HostNameComparisonMode HostNameComparisonMode {
			get { return comparison_mode; }
			set { comparison_mode = value; }
		}

		public int ListenBacklog {
			get { return listen_backlog; }
			set { listen_backlog = value; }
		}

		public long MaxBufferPoolSize {
			get { return max_pool_size; }
			set { max_pool_size = value; }
		}

		public int MaxBufferSize {
			get { return max_buf_size; }
			set { max_buf_size = value; }
		}

		public int MaxConnections {
			get { return max_conn; }
			set { max_conn = value; }
		}

		public long MaxReceivedMessageSize {
			get { return max_msg_size; }
			set { max_msg_size = value; }
		}

		public bool PortSharingEnabled {
			get { return port_sharing_enabled; }
			set { port_sharing_enabled = value; }
		}

		public OptionalReliableSession ReliableSession {
			get { return reliable_session; }
		}

		public XmlDictionaryReaderQuotas ReaderQuotas {
			get { return reader_quotas; }
			set { reader_quotas = value; }
		}

		public NetTcpSecurity Security {
			get { return security; }
		}

		public EnvelopeVersion EnvelopeVersion {
			get { return soap_version; }
		}

		public TransferMode TransferMode {
			get { return transfer_mode; }
			set { transfer_mode = value; }
		}

		public bool TransactionFlow {
			get { return transaction_flow; }
			set { transaction_flow = value; }
		}

		public TransactionProtocol TransactionProtocol {
			get { return transaction_protocol; }
			set { transaction_protocol = value; }
		}

		// overrides

		public override string Scheme {
			get { return "net.tcp"; }
		}

		public override BindingElementCollection CreateBindingElements ()
		{
			BindingElement tx = new TransactionFlowBindingElement (TransactionProtocol.WSAtomicTransactionOctober2004);
			SecurityBindingElement sec = CreateMessageSecurity ();
			BindingElement msg = new BinaryMessageEncodingBindingElement ();
			BindingElement tr = GetTransport ();
			List<BindingElement> list = new List<BindingElement> ();
			if (tx != null)
				list.Add (tx);
			if (sec != null)
				list.Add (sec);
			list.Add (msg);
			list.Add (tr);
			return new BindingElementCollection (list.ToArray ());
		}

		BindingElement GetTransport ()
		{
			return new TcpTransportBindingElement ();
		}

		// based on WSHttpBinding.CreateMessageSecurity()
		SecurityBindingElement CreateMessageSecurity ()
		{
			if (Security.Mode == SecurityMode.Transport ||
			    Security.Mode == SecurityMode.None)
				return null;

			SymmetricSecurityBindingElement element =
				new SymmetricSecurityBindingElement ();

			element.MessageSecurityVersion = MessageSecurityVersion.Default;

			element.SetKeyDerivation (false);

			switch (Security.Message.ClientCredentialType) {
			case MessageCredentialType.Certificate:
				element.EndpointSupportingTokenParameters.Endorsing.Add (
					new X509SecurityTokenParameters ());
				goto default;
			case MessageCredentialType.IssuedToken:
				IssuedSecurityTokenParameters istp =
					new IssuedSecurityTokenParameters ();
				// FIXME: issuer binding must be secure.
				istp.IssuerBinding = new CustomBinding (
					new TextMessageEncodingBindingElement (),
					GetTransport ());
				element.EndpointSupportingTokenParameters.Endorsing.Add (istp);
				goto default;
			case MessageCredentialType.UserName:
				element.EndpointSupportingTokenParameters.SignedEncrypted.Add (
					new UserNameSecurityTokenParameters ());
				goto default;
			case MessageCredentialType.Windows:
				element.ProtectionTokenParameters =
					new KerberosSecurityTokenParameters ();
				break;
			default: // including .None
				X509SecurityTokenParameters p =
					new X509SecurityTokenParameters ();
				p.X509ReferenceStyle = X509KeyIdentifierClauseType.Thumbprint;
				element.ProtectionTokenParameters = p;
				break;
			}

			return element;
		}

		bool IBindingRuntimePreferences.ReceiveSynchronously {
			get { throw new NotImplementedException (); }
		}
	}
}
