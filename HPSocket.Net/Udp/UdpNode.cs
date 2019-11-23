﻿using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HPSocket.Udp
{
    public class UdpNode : IUdpNode
    {
        #region 私有成员

        /// <summary>
        /// 是否释放了
        /// </summary>
        private bool _disposed;

        #endregion

        #region 保护成员

        /// <summary>
        /// 监听对象指针
        /// </summary>
        protected IntPtr ListenerPtr = IntPtr.Zero;

        #endregion

        public UdpNode()
        {
            if (!CreateListener())
            {
                throw new InitializationException("未能正确初始化监听");
            }
        }
        ~UdpNode() => Dispose(false);

        /// <inheritdoc />
        public IntPtr SenderPtr { get; protected set; } = IntPtr.Zero;

        /// <inheritdoc />
        public string Address { get; set; } = "0.0.0.0";

        /// <inheritdoc />
        public ushort Port { get; set; } = 0;

        /// <inheritdoc />
        public event UdpNodePrepareListenEventHandler OnPrepareListen;

        /// <inheritdoc />
        public event UdpNodeSendEventHandler OnSend;

        /// <inheritdoc />
        public event UdpNodeReceiveEventHandler OnReceive;

        /// <inheritdoc />
        public event UdpNodeErrorEventHandler OnError;

        /// <inheritdoc />
        public event UdpNodeShutdownEventHandler OnShutdown;

        /// <inheritdoc />
        public bool HasStarted => SenderPtr != IntPtr.Zero && Sdk.Udp.HP_UdpNode_HasStarted(SenderPtr);

        /// <inheritdoc />
        public ServiceState State => Sdk.Udp.HP_UdpNode_GetState(SenderPtr);

        /// <inheritdoc />
        public CastMode CastMode => Sdk.Udp.HP_UdpNode_GetCastMode(SenderPtr);

        /// <inheritdoc />
        public int PendingDataLength
        {
            get
            {
                var length = 0;
                Sdk.Udp.HP_UdpNode_GetPendingDataLength(SenderPtr, ref length);
                return length;
            }
        }

        /// <inheritdoc />
        public uint MaxDatagramSize
        {
            get => Sdk.Udp.HP_UdpNode_GetMaxDatagramSize(SenderPtr);
            set => Sdk.Udp.HP_UdpNode_SetMaxDatagramSize(SenderPtr, value);
        }

        /// <inheritdoc />
        public bool IsReuseAddress
        {
            get => Sdk.Udp.HP_UdpNode_IsReuseAddress(SenderPtr);
            set => Sdk.Udp.HP_UdpNode_SetReuseAddress(SenderPtr, value);
        }

        /// <inheritdoc />
        public int MultiCastTtl
        {
            get => Sdk.Udp.HP_UdpNode_GetMultiCastTtl(SenderPtr);
            set => Sdk.Udp.HP_UdpNode_SetMultiCastTtl(SenderPtr, value);
        }

        /// <inheritdoc />
        public bool IsMultiCastLoop
        {
            get => Sdk.Udp.HP_UdpNode_IsMultiCastLoop(SenderPtr);
            set => Sdk.Udp.HP_UdpNode_SetMultiCastLoop(SenderPtr, value);
        }

        /// <inheritdoc />
        public uint WorkerThreadCount
        {
            get => Sdk.Udp.HP_UdpNode_GetWorkerThreadCount(SenderPtr);
            set => Sdk.Udp.HP_UdpNode_SetWorkerThreadCount(SenderPtr, value);
        }

        /// <inheritdoc />
        public uint PostReceiveCount
        {
            get => Sdk.Udp.HP_UdpNode_GetPostReceiveCount(SenderPtr);
            set => Sdk.Udp.HP_UdpNode_SetPostReceiveCount(SenderPtr, value);
        }

        /// <inheritdoc />
        public uint FreeBufferPoolSize
        {
            get => Sdk.Udp.HP_UdpNode_GetFreeBufferPoolSize(SenderPtr);
            set => Sdk.Udp.HP_UdpNode_SetFreeBufferPoolSize(SenderPtr, value);
        }

        /// <inheritdoc />
        public uint FreeBufferPoolHold
        {
            get => Sdk.Udp.HP_UdpNode_GetFreeBufferPoolHold(SenderPtr);
            set => Sdk.Udp.HP_UdpNode_SetFreeBufferPoolHold(SenderPtr, value);
        }

        /// <inheritdoc />
        public ReuseAddressPolicy ReuseAddressPolicy
        {
            get => Sdk.Udp.HP_UdpNode_GetReuseAddressPolicy(SenderPtr);
            set => Sdk.Udp.HP_UdpNode_SetReuseAddressPolicy(SenderPtr, value);
        }


        /// <inheritdoc />
        public SocketError ErrorCode => Sdk.Udp.HP_UdpNode_GetLastError(SenderPtr);

        /// <inheritdoc />
        public string Version => Sdk.Sys.GetVersion();

        /// <inheritdoc />
        public string ErrorMessage => Sdk.Udp.HP_UdpNode_GetLastErrorDesc(SenderPtr).PtrToAnsiString();

        /// <inheritdoc />
        public object ExtraData { get; set; }

        /// <inheritdoc />
        public bool Start()

        {
            if (String.IsNullOrWhiteSpace(Address))
            {
                throw new InvalidOperationException("BindAddress属性未设置正确的本机IP地址");
            }

            if (HasStarted)
            {
                return true;
            }

            return Sdk.Udp.HP_UdpNode_Start(SenderPtr, Address, Port);
        }

        /// <inheritdoc />
        public bool StartWithCast(CastMode castMode = CastMode.UniCast, string castAddress = null)
        {
            if (String.IsNullOrWhiteSpace(Address))
            {
                throw new InvalidOperationException("BindAddress属性未设置正确的本机IP地址");
            }

            if (HasStarted)
            {
                return true;
            }

            return Sdk.Udp.HP_UdpNode_StartWithCast(SenderPtr, Address, Port, castMode, castAddress);
        }

        /// <inheritdoc />
        public bool Stop() => HasStarted && Sdk.Udp.HP_UdpNode_Stop(SenderPtr);

        /// <inheritdoc />
        public bool Send(string remoteAddress, ushort remotePort, byte[] data, int length)
        {
            var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
            var ok = Sdk.Udp.HP_UdpNode_Send(SenderPtr, remoteAddress, remotePort, gch.AddrOfPinnedObject(), length);
            gch.Free();
            return ok;
        }

        /// <inheritdoc />
        public bool Send(string remoteAddress, ushort remotePort, byte[] data, int offset, int length)
        {
            var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
            var ok = Sdk.Udp.HP_UdpNode_SendPart(SenderPtr, remoteAddress, remotePort, gch.AddrOfPinnedObject(), length, offset);
            gch.Free();
            return ok;
        }

        /// <inheritdoc />
        public bool Send(string remoteAddress, ushort remotePort, Wsabuf[] buffers, int count) => Sdk.Udp.HP_UdpNode_SendPackets(SenderPtr, remoteAddress, remotePort, buffers, count);

        /// <inheritdoc />
        public bool SendCast(byte[] data, int length)
        {
            var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
            var ok = Sdk.Udp.HP_UdpNode_SendCast(SenderPtr, gch.AddrOfPinnedObject(), length);
            gch.Free();
            return ok;
        }

        /// <inheritdoc />
        public bool SendCast(byte[] data, int offset, int length)
        {
            var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
            var ok = Sdk.Udp.HP_UdpNode_SendCastPart(SenderPtr, gch.AddrOfPinnedObject(), length, offset);
            gch.Free();
            return ok;
        }

        /// <inheritdoc />
        public bool SendCast(Wsabuf[] buffers, int count) => Sdk.Udp.HP_UdpNode_SendCastPackets(SenderPtr, buffers, count);

        /// <inheritdoc />
        public bool GetLocalAddress(out string address, out ushort port)
        {
            var length = 60;
            port = 0;
            var sb = new StringBuilder(length);
            var ok = Sdk.Udp.HP_UdpNode_GetLocalAddress(SenderPtr, sb, ref length, ref port);
            address = ok ? sb.ToString() : string.Empty;
            return ok;
        }

        /// <inheritdoc />
        public bool GetCastAddress(out string address, out ushort port)
        {
            var length = 60;
            port = 0;
            var sb = new StringBuilder(length);
            var ok = Sdk.Udp.HP_UdpNode_GetCastAddress(SenderPtr, sb, ref length, ref port);
            address = ok ? sb.ToString() : string.Empty;
            return ok;
        }


        #region SDK回调

        #region SDK回调委托,防止GC

        private Sdk.UdpNodeOnPrepareListen _onPrepareListen;
        private Sdk.UdpNodeOnSend _onSend;
        private Sdk.UdpNodeOnReceive _onReceive;
        private Sdk.UdpNodeOnError _onError;
        private Sdk.UdpNodeOnShutdown _onShutdown;

        #endregion

        protected virtual void SetCallback()
        {
            _onPrepareListen = SdkOnPrepareListen;
            _onSend = SdkOnSend;
            _onReceive = SdkOnReceive;
            _onError = SdkOnError;
            _onShutdown = SdkOnShutdown;

            Sdk.Udp.HP_Set_FN_UdpNode_OnPrepareListen(ListenerPtr, _onPrepareListen);
            Sdk.Udp.HP_Set_FN_UdpNode_OnSend(ListenerPtr, _onSend);
            Sdk.Udp.HP_Set_FN_UdpNode_OnReceive(ListenerPtr, _onReceive);
            Sdk.Udp.HP_Set_FN_UdpNode_OnError(ListenerPtr, _onError);
            Sdk.Udp.HP_Set_FN_UdpNode_OnShutdown(ListenerPtr, _onShutdown);

            GC.KeepAlive(_onPrepareListen);
            GC.KeepAlive(_onSend);
            GC.KeepAlive(_onReceive);
            GC.KeepAlive(_onError);
            GC.KeepAlive(_onShutdown);
        }

        protected HandleResult SdkOnPrepareListen(IntPtr sender, IntPtr soListen) => OnPrepareListen?.Invoke(this, soListen) ?? HandleResult.Ignore;

        protected HandleResult SdkOnSend(IntPtr sender, string remoteAddress, ushort remotePort, IntPtr data, int length)
        {
            if (OnSend == null) return HandleResult.Ignore;
            var bytes = new byte[length];
            if (bytes.Length > 0)
            {
                Marshal.Copy(data, bytes, 0, length);
            }
            return OnSend(this, remoteAddress, remotePort, bytes);
        }

        protected HandleResult SdkOnReceive(IntPtr sender, string remoteAddress, ushort remotePort, IntPtr data, int length)
        {
            if (OnReceive == null) return HandleResult.Ignore;
            var bytes = new byte[length];
            if (bytes.Length > 0)
            {
                Marshal.Copy(data, bytes, 0, length);
            }
            return OnReceive(this, remoteAddress, remotePort, bytes);
        }

        protected HandleResult SdkOnError(IntPtr sender, SocketOperation socketOperation, int errorCode, string remoteAddress, ushort remotePort, IntPtr data, int length)
        {
            if (OnError == null) return HandleResult.Ignore;
            var bytes = new byte[length];
            if (bytes.Length > 0)
            {
                Marshal.Copy(data, bytes, 0, length);
            }
            return OnError(this, socketOperation, errorCode, remoteAddress, remotePort, bytes);
        }

        protected HandleResult SdkOnShutdown(IntPtr sender) => OnShutdown?.Invoke(this) ?? HandleResult.Ignore;

        #endregion

        /// <summary>
        /// 创建socket监听和服务组件
        /// </summary>
        /// <returns></returns>
        private bool CreateListener()
        {
            if (ListenerPtr != IntPtr.Zero || SenderPtr != IntPtr.Zero)
            {
                return false;
            }

            ListenerPtr = Sdk.Udp.Create_HP_UdpNodeListener();
            if (ListenerPtr == IntPtr.Zero)
            {
                return false;
            }

            SenderPtr = Sdk.Udp.Create_HP_UdpNode(ListenerPtr);
            if (SenderPtr == IntPtr.Zero)
            {
                Destroy();
                return false;
            }

            SetCallback();

            return true;
        }

        /// <summary>
        /// 终止服务并释放资源
        /// </summary>
        private void Destroy()
        {
            Stop();

            if (SenderPtr != IntPtr.Zero)
            {
                Sdk.Udp.Destroy_HP_UdpNode(SenderPtr);
                SenderPtr = IntPtr.Zero;
            }

            if (ListenerPtr != IntPtr.Zero)
            {
                Sdk.Udp.Destroy_HP_UdpNodeListener(ListenerPtr);
                ListenerPtr = IntPtr.Zero;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // 释放托管对象资源
            }
            Destroy();

            _disposed = true;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
