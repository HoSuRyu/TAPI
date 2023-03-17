﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TAPI_Interviewer.Models
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="SEND_MASTER")]
	public partial class SENDMASTERDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region 확장성 메서드 정의
    partial void OnCreated();
    partial void InsertT_SendMessage(T_SendMessage instance);
    partial void UpdateT_SendMessage(T_SendMessage instance);
    partial void DeleteT_SendMessage(T_SendMessage instance);
    partial void InsertT_SendMaster(T_SendMaster instance);
    partial void UpdateT_SendMaster(T_SendMaster instance);
    partial void DeleteT_SendMaster(T_SendMaster instance);
    partial void InsertT_SendEmail(T_SendEmail instance);
    partial void UpdateT_SendEmail(T_SendEmail instance);
    partial void DeleteT_SendEmail(T_SendEmail instance);
    #endregion
		
		public SENDMASTERDataContext() : 
				base(global::System.Configuration.ConfigurationManager.ConnectionStrings["SEND_MASTERConnectionString1"].ConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public SENDMASTERDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public SENDMASTERDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public SENDMASTERDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public SENDMASTERDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<T_SendMessage> T_SendMessage
		{
			get
			{
				return this.GetTable<T_SendMessage>();
			}
		}
		
		public System.Data.Linq.Table<T_SendMaster> T_SendMaster
		{
			get
			{
				return this.GetTable<T_SendMaster>();
			}
		}
		
		public System.Data.Linq.Table<T_SendEmail> T_SendEmail
		{
			get
			{
				return this.GetTable<T_SendEmail>();
			}
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.PROC_SEND_MESSAGE")]
		public ISingleResult<PROC_SEND_MESSAGEResult> PROC_SEND_MESSAGE([global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_SWID", DbType="VarChar(50)")] string p_SWID, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_FROM_TEL", DbType="VarChar(50)")] string p_FROM_TEL, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_TO_TEL", DbType="VarChar(500)")] string p_TO_TEL, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_SUBJECT", DbType="VarChar(500)")] string p_SUBJECT, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_BODY", DbType="VarChar(MAX)")] string p_BODY, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_REQUEST_TIME", DbType="DateTime")] System.Nullable<System.DateTime> p_REQUEST_TIME, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_ETC", DbType="VarChar(1000)")] string p_ETC, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="POF", DbType="VarChar(50)")] string pOF)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), p_SWID, p_FROM_TEL, p_TO_TEL, p_SUBJECT, p_BODY, p_REQUEST_TIME, p_ETC, pOF);
			return ((ISingleResult<PROC_SEND_MESSAGEResult>)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.PROC_SEND_EMAIL")]
		public ISingleResult<PROC_SEND_EMAILResult> PROC_SEND_EMAIL([global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_SWID", DbType="VarChar(50)")] string p_SWID, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_FROM_ADERESS", DbType="VarChar(50)")] string p_FROM_ADERESS, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_TO_ADERESS", DbType="VarChar(500)")] string p_TO_ADERESS, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_SUBJECT", DbType="VarChar(500)")] string p_SUBJECT, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_BODY", DbType="VarChar(MAX)")] string p_BODY, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_CC_ADERESS", DbType="VarChar(500)")] string p_CC_ADERESS, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_BCC_ADERESS", DbType="VarChar(500)")] string p_BCC_ADERESS, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_REQUEST_TIME", DbType="DateTime")] System.Nullable<System.DateTime> p_REQUEST_TIME, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_ETC", DbType="VarChar(1000)")] string p_ETC, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="P_ATTACHMENTS", DbType="VarChar(500)")] string p_ATTACHMENTS, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="POF", DbType="VarChar(50)")] string pOF)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), p_SWID, p_FROM_ADERESS, p_TO_ADERESS, p_SUBJECT, p_BODY, p_CC_ADERESS, p_BCC_ADERESS, p_REQUEST_TIME, p_ETC, p_ATTACHMENTS, pOF);
			return ((ISingleResult<PROC_SEND_EMAILResult>)(result.ReturnValue));
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.T_SendMessage")]
	public partial class T_SendMessage : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private decimal _id;
		
		private decimal _sendKey;
		
		private string _fromtel;
		
		private string _totel;
		
		private string _subject;
		
		private string _body;
		
		private string _etc;
		
		private string _POF;
		
    #region 확장성 메서드 정의
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(decimal value);
    partial void OnidChanged();
    partial void OnsendKeyChanging(decimal value);
    partial void OnsendKeyChanged();
    partial void OnfromtelChanging(string value);
    partial void OnfromtelChanged();
    partial void OntotelChanging(string value);
    partial void OntotelChanged();
    partial void OnsubjectChanging(string value);
    partial void OnsubjectChanged();
    partial void OnbodyChanging(string value);
    partial void OnbodyChanged();
    partial void OnetcChanging(string value);
    partial void OnetcChanged();
    partial void OnPOFChanging(string value);
    partial void OnPOFChanged();
    #endregion
		
		public T_SendMessage()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id", AutoSync=AutoSync.OnInsert, DbType="Decimal(38,0) NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public decimal id
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((this._id != value))
				{
					this.OnidChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("id");
					this.OnidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_sendKey", DbType="Decimal(38,0) NOT NULL")]
		public decimal sendKey
		{
			get
			{
				return this._sendKey;
			}
			set
			{
				if ((this._sendKey != value))
				{
					this.OnsendKeyChanging(value);
					this.SendPropertyChanging();
					this._sendKey = value;
					this.SendPropertyChanged("sendKey");
					this.OnsendKeyChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_fromtel", DbType="VarChar(20) NOT NULL", CanBeNull=false)]
		public string fromtel
		{
			get
			{
				return this._fromtel;
			}
			set
			{
				if ((this._fromtel != value))
				{
					this.OnfromtelChanging(value);
					this.SendPropertyChanging();
					this._fromtel = value;
					this.SendPropertyChanged("fromtel");
					this.OnfromtelChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_totel", DbType="VarChar(100) NOT NULL", CanBeNull=false)]
		public string totel
		{
			get
			{
				return this._totel;
			}
			set
			{
				if ((this._totel != value))
				{
					this.OntotelChanging(value);
					this.SendPropertyChanging();
					this._totel = value;
					this.SendPropertyChanged("totel");
					this.OntotelChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_subject", DbType="VarChar(500) NOT NULL", CanBeNull=false)]
		public string subject
		{
			get
			{
				return this._subject;
			}
			set
			{
				if ((this._subject != value))
				{
					this.OnsubjectChanging(value);
					this.SendPropertyChanging();
					this._subject = value;
					this.SendPropertyChanged("subject");
					this.OnsubjectChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_body", DbType="VarChar(MAX)")]
		public string body
		{
			get
			{
				return this._body;
			}
			set
			{
				if ((this._body != value))
				{
					this.OnbodyChanging(value);
					this.SendPropertyChanging();
					this._body = value;
					this.SendPropertyChanged("body");
					this.OnbodyChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_etc", DbType="VarChar(1000)")]
		public string etc
		{
			get
			{
				return this._etc;
			}
			set
			{
				if ((this._etc != value))
				{
					this.OnetcChanging(value);
					this.SendPropertyChanging();
					this._etc = value;
					this.SendPropertyChanged("etc");
					this.OnetcChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_POF", DbType="VarChar(50)")]
		public string POF
		{
			get
			{
				return this._POF;
			}
			set
			{
				if ((this._POF != value))
				{
					this.OnPOFChanging(value);
					this.SendPropertyChanging();
					this._POF = value;
					this.SendPropertyChanged("POF");
					this.OnPOFChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.T_SendMaster")]
	public partial class T_SendMaster : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private decimal _sendKey;
		
		private int _sendType;
		
		private System.DateTime _requestTime;
		
		private System.Nullable<int> _status;
		
		private string _etc;
		
		private string _SWID;
		
		private string _POF;
		
		private string _popbillSendKey;
		
		private string _popbillSendErrorCode;
		
		private string _popbillSendErrorMessage;
		
		private string _popbillResultCode;
		
		private string _popbillResultMessage;
		
		private string _popbillFaxPageCount;
		
    #region 확장성 메서드 정의
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnsendKeyChanging(decimal value);
    partial void OnsendKeyChanged();
    partial void OnsendTypeChanging(int value);
    partial void OnsendTypeChanged();
    partial void OnrequestTimeChanging(System.DateTime value);
    partial void OnrequestTimeChanged();
    partial void OnstatusChanging(System.Nullable<int> value);
    partial void OnstatusChanged();
    partial void OnetcChanging(string value);
    partial void OnetcChanged();
    partial void OnSWIDChanging(string value);
    partial void OnSWIDChanged();
    partial void OnPOFChanging(string value);
    partial void OnPOFChanged();
    partial void OnpopbillSendKeyChanging(string value);
    partial void OnpopbillSendKeyChanged();
    partial void OnpopbillSendErrorCodeChanging(string value);
    partial void OnpopbillSendErrorCodeChanged();
    partial void OnpopbillSendErrorMessageChanging(string value);
    partial void OnpopbillSendErrorMessageChanged();
    partial void OnpopbillResultCodeChanging(string value);
    partial void OnpopbillResultCodeChanged();
    partial void OnpopbillResultMessageChanging(string value);
    partial void OnpopbillResultMessageChanged();
    partial void OnpopbillFaxPageCountChanging(string value);
    partial void OnpopbillFaxPageCountChanged();
    #endregion
		
		public T_SendMaster()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_sendKey", AutoSync=AutoSync.OnInsert, DbType="Decimal(38,0) NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public decimal sendKey
		{
			get
			{
				return this._sendKey;
			}
			set
			{
				if ((this._sendKey != value))
				{
					this.OnsendKeyChanging(value);
					this.SendPropertyChanging();
					this._sendKey = value;
					this.SendPropertyChanged("sendKey");
					this.OnsendKeyChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_sendType", DbType="Int NOT NULL")]
		public int sendType
		{
			get
			{
				return this._sendType;
			}
			set
			{
				if ((this._sendType != value))
				{
					this.OnsendTypeChanging(value);
					this.SendPropertyChanging();
					this._sendType = value;
					this.SendPropertyChanged("sendType");
					this.OnsendTypeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_requestTime", DbType="DateTime NOT NULL")]
		public System.DateTime requestTime
		{
			get
			{
				return this._requestTime;
			}
			set
			{
				if ((this._requestTime != value))
				{
					this.OnrequestTimeChanging(value);
					this.SendPropertyChanging();
					this._requestTime = value;
					this.SendPropertyChanged("requestTime");
					this.OnrequestTimeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_status", DbType="Int")]
		public System.Nullable<int> status
		{
			get
			{
				return this._status;
			}
			set
			{
				if ((this._status != value))
				{
					this.OnstatusChanging(value);
					this.SendPropertyChanging();
					this._status = value;
					this.SendPropertyChanged("status");
					this.OnstatusChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_etc", DbType="VarChar(1000)")]
		public string etc
		{
			get
			{
				return this._etc;
			}
			set
			{
				if ((this._etc != value))
				{
					this.OnetcChanging(value);
					this.SendPropertyChanging();
					this._etc = value;
					this.SendPropertyChanged("etc");
					this.OnetcChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SWID", DbType="VarChar(50)")]
		public string SWID
		{
			get
			{
				return this._SWID;
			}
			set
			{
				if ((this._SWID != value))
				{
					this.OnSWIDChanging(value);
					this.SendPropertyChanging();
					this._SWID = value;
					this.SendPropertyChanged("SWID");
					this.OnSWIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_POF", DbType="VarChar(50)")]
		public string POF
		{
			get
			{
				return this._POF;
			}
			set
			{
				if ((this._POF != value))
				{
					this.OnPOFChanging(value);
					this.SendPropertyChanging();
					this._POF = value;
					this.SendPropertyChanged("POF");
					this.OnPOFChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_popbillSendKey", DbType="VarChar(100)")]
		public string popbillSendKey
		{
			get
			{
				return this._popbillSendKey;
			}
			set
			{
				if ((this._popbillSendKey != value))
				{
					this.OnpopbillSendKeyChanging(value);
					this.SendPropertyChanging();
					this._popbillSendKey = value;
					this.SendPropertyChanged("popbillSendKey");
					this.OnpopbillSendKeyChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_popbillSendErrorCode", DbType="VarChar(100)")]
		public string popbillSendErrorCode
		{
			get
			{
				return this._popbillSendErrorCode;
			}
			set
			{
				if ((this._popbillSendErrorCode != value))
				{
					this.OnpopbillSendErrorCodeChanging(value);
					this.SendPropertyChanging();
					this._popbillSendErrorCode = value;
					this.SendPropertyChanged("popbillSendErrorCode");
					this.OnpopbillSendErrorCodeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_popbillSendErrorMessage", DbType="VarChar(1000)")]
		public string popbillSendErrorMessage
		{
			get
			{
				return this._popbillSendErrorMessage;
			}
			set
			{
				if ((this._popbillSendErrorMessage != value))
				{
					this.OnpopbillSendErrorMessageChanging(value);
					this.SendPropertyChanging();
					this._popbillSendErrorMessage = value;
					this.SendPropertyChanged("popbillSendErrorMessage");
					this.OnpopbillSendErrorMessageChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_popbillResultCode", DbType="VarChar(100)")]
		public string popbillResultCode
		{
			get
			{
				return this._popbillResultCode;
			}
			set
			{
				if ((this._popbillResultCode != value))
				{
					this.OnpopbillResultCodeChanging(value);
					this.SendPropertyChanging();
					this._popbillResultCode = value;
					this.SendPropertyChanged("popbillResultCode");
					this.OnpopbillResultCodeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_popbillResultMessage", DbType="VarChar(1000)")]
		public string popbillResultMessage
		{
			get
			{
				return this._popbillResultMessage;
			}
			set
			{
				if ((this._popbillResultMessage != value))
				{
					this.OnpopbillResultMessageChanging(value);
					this.SendPropertyChanging();
					this._popbillResultMessage = value;
					this.SendPropertyChanged("popbillResultMessage");
					this.OnpopbillResultMessageChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_popbillFaxPageCount", DbType="VarChar(10)")]
		public string popbillFaxPageCount
		{
			get
			{
				return this._popbillFaxPageCount;
			}
			set
			{
				if ((this._popbillFaxPageCount != value))
				{
					this.OnpopbillFaxPageCountChanging(value);
					this.SendPropertyChanging();
					this._popbillFaxPageCount = value;
					this.SendPropertyChanged("popbillFaxPageCount");
					this.OnpopbillFaxPageCountChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.T_SendEmail")]
	public partial class T_SendEmail : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private decimal _id;
		
		private decimal _sendKey;
		
		private string _fromAddress;
		
		private string _toAddress;
		
		private string _subject;
		
		private string _body;
		
		private string _ccAddress;
		
		private string _bccAddress;
		
		private string _attachments;
		
		private string _etc;
		
		private string _POF;
		
    #region 확장성 메서드 정의
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(decimal value);
    partial void OnidChanged();
    partial void OnsendKeyChanging(decimal value);
    partial void OnsendKeyChanged();
    partial void OnfromAddressChanging(string value);
    partial void OnfromAddressChanged();
    partial void OntoAddressChanging(string value);
    partial void OntoAddressChanged();
    partial void OnsubjectChanging(string value);
    partial void OnsubjectChanged();
    partial void OnbodyChanging(string value);
    partial void OnbodyChanged();
    partial void OnccAddressChanging(string value);
    partial void OnccAddressChanged();
    partial void OnbccAddressChanging(string value);
    partial void OnbccAddressChanged();
    partial void OnattachmentsChanging(string value);
    partial void OnattachmentsChanged();
    partial void OnetcChanging(string value);
    partial void OnetcChanged();
    partial void OnPOFChanging(string value);
    partial void OnPOFChanged();
    #endregion
		
		public T_SendEmail()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id", AutoSync=AutoSync.OnInsert, DbType="Decimal(38,0) NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public decimal id
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((this._id != value))
				{
					this.OnidChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("id");
					this.OnidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_sendKey", DbType="Decimal(38,0) NOT NULL")]
		public decimal sendKey
		{
			get
			{
				return this._sendKey;
			}
			set
			{
				if ((this._sendKey != value))
				{
					this.OnsendKeyChanging(value);
					this.SendPropertyChanging();
					this._sendKey = value;
					this.SendPropertyChanged("sendKey");
					this.OnsendKeyChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_fromAddress", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string fromAddress
		{
			get
			{
				return this._fromAddress;
			}
			set
			{
				if ((this._fromAddress != value))
				{
					this.OnfromAddressChanging(value);
					this.SendPropertyChanging();
					this._fromAddress = value;
					this.SendPropertyChanged("fromAddress");
					this.OnfromAddressChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_toAddress", DbType="VarChar(500) NOT NULL", CanBeNull=false)]
		public string toAddress
		{
			get
			{
				return this._toAddress;
			}
			set
			{
				if ((this._toAddress != value))
				{
					this.OntoAddressChanging(value);
					this.SendPropertyChanging();
					this._toAddress = value;
					this.SendPropertyChanged("toAddress");
					this.OntoAddressChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_subject", DbType="VarChar(500) NOT NULL", CanBeNull=false)]
		public string subject
		{
			get
			{
				return this._subject;
			}
			set
			{
				if ((this._subject != value))
				{
					this.OnsubjectChanging(value);
					this.SendPropertyChanging();
					this._subject = value;
					this.SendPropertyChanged("subject");
					this.OnsubjectChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_body", DbType="VarChar(MAX)")]
		public string body
		{
			get
			{
				return this._body;
			}
			set
			{
				if ((this._body != value))
				{
					this.OnbodyChanging(value);
					this.SendPropertyChanging();
					this._body = value;
					this.SendPropertyChanged("body");
					this.OnbodyChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ccAddress", DbType="VarChar(500)")]
		public string ccAddress
		{
			get
			{
				return this._ccAddress;
			}
			set
			{
				if ((this._ccAddress != value))
				{
					this.OnccAddressChanging(value);
					this.SendPropertyChanging();
					this._ccAddress = value;
					this.SendPropertyChanged("ccAddress");
					this.OnccAddressChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_bccAddress", DbType="VarChar(500)")]
		public string bccAddress
		{
			get
			{
				return this._bccAddress;
			}
			set
			{
				if ((this._bccAddress != value))
				{
					this.OnbccAddressChanging(value);
					this.SendPropertyChanging();
					this._bccAddress = value;
					this.SendPropertyChanged("bccAddress");
					this.OnbccAddressChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_attachments", DbType="VarChar(500)")]
		public string attachments
		{
			get
			{
				return this._attachments;
			}
			set
			{
				if ((this._attachments != value))
				{
					this.OnattachmentsChanging(value);
					this.SendPropertyChanging();
					this._attachments = value;
					this.SendPropertyChanged("attachments");
					this.OnattachmentsChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_etc", DbType="VarChar(1000)")]
		public string etc
		{
			get
			{
				return this._etc;
			}
			set
			{
				if ((this._etc != value))
				{
					this.OnetcChanging(value);
					this.SendPropertyChanging();
					this._etc = value;
					this.SendPropertyChanged("etc");
					this.OnetcChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_POF", DbType="VarChar(50)")]
		public string POF
		{
			get
			{
				return this._POF;
			}
			set
			{
				if ((this._POF != value))
				{
					this.OnPOFChanging(value);
					this.SendPropertyChanging();
					this._POF = value;
					this.SendPropertyChanged("POF");
					this.OnPOFChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	public partial class PROC_SEND_MESSAGEResult
	{
		
		private string _sendKey;
		
		public PROC_SEND_MESSAGEResult()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_sendKey", DbType="NChar(20)")]
		public string sendKey
		{
			get
			{
				return this._sendKey;
			}
			set
			{
				if ((this._sendKey != value))
				{
					this._sendKey = value;
				}
			}
		}
	}
	
	public partial class PROC_SEND_EMAILResult
	{
		
		private string _sendKey;
		
		public PROC_SEND_EMAILResult()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_sendKey", DbType="NChar(20)")]
		public string sendKey
		{
			get
			{
				return this._sendKey;
			}
			set
			{
				if ((this._sendKey != value))
				{
					this._sendKey = value;
				}
			}
		}
	}
}
#pragma warning restore 1591