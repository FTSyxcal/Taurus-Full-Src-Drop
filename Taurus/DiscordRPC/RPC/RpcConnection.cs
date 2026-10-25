
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using Taurus.DiscordRPC.Converters;
using Taurus.DiscordRPC.Events;
using Taurus.DiscordRPC.Helper;
using Taurus.DiscordRPC.IO;
using Taurus.DiscordRPC.Logging;
using Taurus.DiscordRPC.Message;
using Taurus.DiscordRPC.RPC.Commands;
using Taurus.DiscordRPC.RPC.Payload;
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC.RPC;
internal class RpcConnection : IDisposable
{
  public static readonly int VERSION = 1;
  public static readonly int POLL_RATE = 1000;
  private static readonly bool CLEAR_ON_SHUTDOWN = true;
  private static readonly bool LOCK_STEP = false;
  private ILogger _logger;
  private RpcState _state;
  private readonly object l_states = new object();
  private Configuration _configuration;
  private readonly object l_config = new object();
  private volatile bool aborting;
  private volatile bool shutdown;
  private readonly string applicationID;
  private readonly int processID;
  private long nonce;
  private Thread thread;
  private readonly INamedPipeClient namedPipe;
  private readonly int targetPipe;
  private readonly object l_rtqueue = new object();
  private readonly uint _maxRtQueueSize;
  private readonly Queue<ICommand> _rtqueue;
  private readonly object l_rxqueue = new object();
  private readonly uint _maxRxQueueSize;
  private readonly Queue<IMessage> _rxqueue;
  private readonly AutoResetEvent queueUpdatedEvent = new AutoResetEvent(false);
  private readonly BackoffDelay delay;
  public ILogger Logger
  {
    get => this._logger;
    set
    {
      this._logger = value;
      if (this.namedPipe == null)
        return;
      this.namedPipe.Logger = value;
    }
  }
  public RpcState State
  {
    get
    {
      lock (this.l_states)
        return this._state;
    }
  }
  public Configuration Configuration
  {
    get
    {
      lock (this.l_config)
        return this._configuration;
    }
  }
  public bool IsRunning => this.thread != null;
  public bool ShutdownOnly { get; set; }
  public event OnRpcMessageEvent OnRpcMessage;
  public RpcConnection(
    string applicationID,
    int processID,
    int targetPipe,
    INamedPipeClient client,
    uint maxRxQueueSize = 128 ,
    uint maxRtQueueSize = 512 )
  {
    this.applicationID = applicationID;
    this.processID = processID;
    this.targetPipe = targetPipe;
    this.namedPipe = client;
    this.ShutdownOnly = true;
    this.Logger = (ILogger) new NullLogger();
    this.delay = new BackoffDelay(500, 60000);
    this._maxRtQueueSize = maxRtQueueSize;
    this._rtqueue = new Queue<ICommand>((int) this._maxRtQueueSize + 1);
    this._maxRxQueueSize = maxRxQueueSize;
    this._rxqueue = new Queue<IMessage>((int) this._maxRxQueueSize + 1);
    this.nonce = 0L;
  }
  private long GetNextNonce()
  {
    ++this.nonce;
    return this.nonce;
  }
  internal void EnqueueCommand(ICommand command)
  {
    this.Logger.Trace("Enqueue Command: {0}", (object) command.GetType().FullName);
    if (this.aborting || this.shutdown)
      return;
    lock (this.l_rtqueue)
    {
      if ((long) this._rtqueue.Count == (long) this._maxRtQueueSize)
      {
        this.Logger.Error("Too many enqueued commands, dropping oldest one. Maybe you are pushing new presences to fast?");
        this._rtqueue.Dequeue();
      }
      this._rtqueue.Enqueue(command);
    }
  }
  private void EnqueueMessage(IMessage message)
  {
    try
    {
      if (this.OnRpcMessage != null)
        this.OnRpcMessage((object) this, message);
    }
    catch (Exception ex)
    {
      this.Logger.Error("Unhandled Exception while processing event: {0}", (object) ex.GetType().FullName);
      this.Logger.Error(ex.Message);
      this.Logger.Error(ex.StackTrace);
    }
    if (this._maxRxQueueSize == 0U)
    {
      this.Logger.Trace("Enqueued Message, but queue size is 0.");
    }
    else
    {
      this.Logger.Trace("Enqueue Message: {0}", (object) message.Type);
      lock (this.l_rxqueue)
      {
        if ((long) this._rxqueue.Count == (long) this._maxRxQueueSize)
        {
          this.Logger.Warning("Too many enqueued messages, dropping oldest one.");
          this._rxqueue.Dequeue();
        }
        this._rxqueue.Enqueue(message);
      }
    }
  }
  internal IMessage DequeueMessage()
  {
    lock (this.l_rxqueue)
      return this._rxqueue.Count == 0 ? (IMessage) null : this._rxqueue.Dequeue();
  }
  internal IMessage[] DequeueMessages()
  {
    lock (this.l_rxqueue)
    {
      IMessage[] array = this._rxqueue.ToArray();
      this._rxqueue.Clear();
      return array;
    }
  }
  private void MainLoop()
  {
    this.Logger.Info("RPC Connection Started");
    if (this.Logger.Level <= LogLevel.Trace)
    {
      this.Logger.Trace("============================");
      this.Logger.Trace("Assembly:             " + Assembly.GetAssembly(typeof (RichPresence)).FullName);
      this.Logger.Trace("Pipe:                 " + this.namedPipe.GetType().FullName);
      this.Logger.Trace("Platform:             " + Environment.OSVersion?.ToString());
      this.Logger.Trace("DotNet:               " + Environment.Version?.ToString());
      this.Logger.Trace("applicationID:        " + this.applicationID);
      this.Logger.Trace("targetPipe:           " + this.targetPipe.ToString());
      this.Logger.Trace("POLL_RATE:            " + RpcConnection.POLL_RATE.ToString());
      this.Logger.Trace("_maxRtQueueSize:      " + this._maxRtQueueSize.ToString());
      this.Logger.Trace("_maxRxQueueSize:      " + this._maxRxQueueSize.ToString());
      this.Logger.Trace("============================");
    }
    while (!this.aborting)
    {
      if (!this.shutdown)
      {
        try
        {
          if (this.namedPipe == null)
          {
            this.Logger.Error("Something bad has happened with our pipe client!");
            this.aborting = true;
            return;
          }
          this.Logger.Trace("Connecting to the pipe through the {0}", (object) this.namedPipe.GetType().FullName);
          if (this.namedPipe.Connect(this.targetPipe))
          {
            this.Logger.Trace("Connected to the pipe. Attempting to establish handshake...");
            this.EnqueueMessage((IMessage) new ConnectionEstablishedMessage()
            {
              ConnectedPipe = this.namedPipe.ConnectedPipe
            });
            this.EstablishHandshake();
            this.Logger.Trace("Connection Established. Starting reading loop...");
            bool flag = true;
            while (flag && !this.aborting && !this.shutdown && this.namedPipe.IsConnected)
            {
              PipeFrame frame;
              if (this.namedPipe.ReadFrame(out frame))
              {
                this.Logger.Trace("Read Payload: {0}", (object) frame.Opcode);
                switch (frame.Opcode)
                {
                  case Opcode.Frame:
                    if (this.shutdown)
                    {
                      this.Logger.Warning("Skipping frame because we are shutting down.");
                      break;
                    }
                    if (frame.Data == null)
                    {
                      this.Logger.Error("We received no data from the frame so we cannot get the event payload!");
                      break;
                    }
                    EventPayload response = (EventPayload) null;
                    try
                    {
                      response = frame.GetObject<EventPayload>();
                    }
                    catch (Exception ex)
                    {
                      this.Logger.Error("Failed to parse event! {0}", (object) ex.Message);
                      this.Logger.Error("Data: {0}", (object) frame.Message);
                    }
                    try
                    {
                      if (response != null)
                      {
                        this.ProcessFrame(response);
                        break;
                      }
                      break;
                    }
                    catch (Exception ex)
                    {
                      this.Logger.Error("Failed to process event! {0}", (object) ex.Message);
                      this.Logger.Error("Data: {0}", (object) frame.Message);
                      break;
                    }
                  case Opcode.Close:
                    ClosePayload closePayload = frame.GetObject<ClosePayload>();
                    this.Logger.Warning("We have been told to terminate by discord: ({0}) {1}", (object) closePayload.Code, (object) closePayload.Reason);
                    this.EnqueueMessage((IMessage) new CloseMessage()
                    {
                      Code = closePayload.Code,
                      Reason = closePayload.Reason
                    });
                    flag = false;
                    break;
                  case Opcode.Ping:
                    this.Logger.Trace("PING");
                    frame.Opcode = Opcode.Pong;
                    this.namedPipe.WriteFrame(frame);
                    break;
                  case Opcode.Pong:
                    this.Logger.Trace("PONG");
                    break;
                  default:
                    this.Logger.Error("Invalid opcode: {0}", (object) frame.Opcode);
                    flag = false;
                    break;
                }
              }
              if (!this.aborting && this.namedPipe.IsConnected)
              {
                this.ProcessCommandQueue();
                this.queueUpdatedEvent.WaitOne(RpcConnection.POLL_RATE);
              }
            }
            this.Logger.Trace("Left main read loop for some reason. Aborting: {0}, Shutting Down: {1}", (object) this.aborting, (object) this.shutdown);
          }
          else
          {
            this.Logger.Error("Failed to connect for some reason.");
            this.EnqueueMessage((IMessage) new ConnectionFailedMessage()
            {
              FailedPipe = this.targetPipe
            });
          }
          if (!this.aborting)
          {
            if (!this.shutdown)
            {
              this.Logger.Trace("Waiting {0}ms before attempting to connect again", (object) (long) this.delay.NextDelay());
              Thread.Sleep(this.delay.NextDelay());
            }
          }
        }
        catch (Exception ex)
        {
          this.Logger.Error("Unhandled Exception: {0}", (object) ex.GetType().FullName);
          this.Logger.Error(ex.Message);
          this.Logger.Error(ex.StackTrace);
        }
        finally
        {
          if (this.namedPipe.IsConnected)
          {
            this.Logger.Trace("Closing the named pipe.");
            this.namedPipe.Close();
          }
          this.SetConnectionState(RpcState.Disconnected);
        }
      }
      else
        break;
    }
    this.Logger.Trace("Left Main Loop");
    if (this.namedPipe != null)
      this.namedPipe.Dispose();
    this.Logger.Info("Thread Terminated, no longer performing RPC connection.");
  }
  private void ProcessFrame(EventPayload response)
  {
    this.Logger.Info("Handling Response. Cmd: {0}, Event: {1}", (object) response.Command, (object) response.Event);
    ServerEvent? nullable;
    if (response.Event.HasValue)
    {
      nullable = response.Event;
      if (nullable.Value == ServerEvent.Error)
      {
        this.Logger.Error("Error received from the RPC");
        ErrorMessage message = response.GetObject<ErrorMessage>();
        this.Logger.Error("Server responded with an error message: ({0}) {1}", (object) message.Code.ToString(), (object) message.Message);
        this.EnqueueMessage((IMessage) message);
        return;
      }
    }
    if (this.State == RpcState.Connecting && response.Command == Command.Dispatch)
    {
      nullable = response.Event;
      if (nullable.HasValue)
      {
        nullable = response.Event;
        if (nullable.Value == ServerEvent.Ready)
        {
          this.Logger.Info("Connection established with the RPC");
          this.SetConnectionState(RpcState.Connected);
          this.delay.Reset();
          ReadyMessage message = response.GetObject<ReadyMessage>();
          lock (this.l_config)
          {
            this._configuration = message.Configuration;
            message.User.SetConfiguration(this._configuration);
          }
          this.EnqueueMessage((IMessage) message);
          return;
        }
      }
    }
    if (this.State == RpcState.Connected)
    {
      switch (response.Command)
      {
        case Command.Dispatch:
          this.ProcessDispatch(response);
          break;
        case Command.SetActivity:
          if (response.Data == null)
          {
            this.EnqueueMessage((IMessage) new PresenceMessage());
            break;
          }
          this.EnqueueMessage((IMessage) new PresenceMessage(response.GetObject<RichPresenceResponse>()));
          break;
        case Command.Subscribe:
        case Command.Unsubscribe:
          ((Collection<JsonConverter>) new JsonSerializer().Converters).Add((JsonConverter) new EnumSnakeCaseConverter());
          nullable = response.GetObject<EventPayload>().Event;
          ServerEvent evt = nullable.Value;
          if (response.Command == Command.Subscribe)
          {
            this.EnqueueMessage((IMessage) new SubscribeMessage(evt));
            break;
          }
          this.EnqueueMessage((IMessage) new UnsubscribeMessage(evt));
          break;
        case Command.SendActivityJoinInvite:
          this.Logger.Trace("Got invite response ack.");
          break;
        case Command.CloseActivityJoinRequest:
          this.Logger.Trace("Got invite response reject ack.");
          break;
        default:
          this.Logger.Error("Unknown frame was received! {0}", (object) response.Command);
          break;
      }
    }
    else
      this.Logger.Trace("Received a frame while we are disconnected. Ignoring. Cmd: {0}, Event: {1}", (object) response.Command, (object) response.Event);
  }
  private void ProcessDispatch(EventPayload response)
  {
    if (response.Command != Command.Dispatch || !response.Event.HasValue)
      return;
    switch (response.Event.Value)
    {
      case ServerEvent.ActivityJoin:
        this.EnqueueMessage((IMessage) response.GetObject<JoinMessage>());
        break;
      case ServerEvent.ActivitySpectate:
        this.EnqueueMessage((IMessage) response.GetObject<SpectateMessage>());
        break;
      case ServerEvent.ActivityJoinRequest:
        this.EnqueueMessage((IMessage) response.GetObject<JoinRequestMessage>());
        break;
      default:
        this.Logger.Warning("Ignoring {0}", (object) response.Event.Value);
        break;
    }
  }
  private void ProcessCommandQueue()
  {
    if (this.State != RpcState.Connected)
      return;
    if (this.aborting)
      this.Logger.Warning("We have been told to write a queue but we have also been aborted.");
    bool flag = true;
    ICommand command = (ICommand) null;
    while (flag && this.namedPipe.IsConnected)
    {
      lock (this.l_rtqueue)
      {
        flag = this._rtqueue.Count > 0;
        if (!flag)
          break;
        command = this._rtqueue.Peek();
      }
      if (this.shutdown || !this.aborting && RpcConnection.LOCK_STEP)
        flag = false;
      IPayload ipayload = command.PreparePayload(this.GetNextNonce());
      this.Logger.Trace("Attempting to send payload: {0}", (object) ipayload.Command);
      PipeFrame frame = new PipeFrame();
      if (command is CloseCommand)
      {
        this.SendHandwave();
        this.Logger.Trace("Handwave sent, ending queue processing.");
        lock (this.l_rtqueue)
        {
          this._rtqueue.Dequeue();
          break;
        }
      }
      if (this.aborting)
      {
        this.Logger.Warning("- skipping frame because of abort.");
        lock (this.l_rtqueue)
          this._rtqueue.Dequeue();
      }
      else
      {
        frame.SetObject(Opcode.Frame, (object) ipayload);
        this.Logger.Trace("Sending payload: {0}", (object) ipayload.Command);
        if (this.namedPipe.WriteFrame(frame))
        {
          this.Logger.Trace("Sent Successfully.");
          lock (this.l_rtqueue)
            this._rtqueue.Dequeue();
        }
        else
        {
          this.Logger.Warning("Something went wrong during writing!");
          break;
        }
      }
    }
  }
  private void EstablishHandshake()
  {
    this.Logger.Trace("Attempting to establish a handshake...");
    if (this.State != RpcState.Disconnected)
    {
      this.Logger.Error("State must be disconnected in order to start a handshake!");
    }
    else
    {
      this.Logger.Trace("Sending Handshake...");
      if (!this.namedPipe.WriteFrame(new PipeFrame(Opcode.Handshake, (object) new Handshake()
      {
        Version = RpcConnection.VERSION,
        ClientID = this.applicationID
      })))
        this.Logger.Error("Failed to write a handshake.");
      else
        this.SetConnectionState(RpcState.Connecting);
    }
  }
  private void SendHandwave()
  {
    this.Logger.Info("Attempting to wave goodbye...");
    if (this.State == RpcState.Disconnected)
    {
      this.Logger.Error("State must NOT be disconnected in order to send a handwave!");
    }
    else
    {
      if (this.namedPipe.WriteFrame(new PipeFrame(Opcode.Close, (object) new Handshake()
      {
        Version = RpcConnection.VERSION,
        ClientID = this.applicationID
      })))
        return;
      this.Logger.Error("failed to write a handwave.");
    }
  }
  public bool AttemptConnection()
  {
    this.Logger.Info("Attempting a new connection");
    if (this.thread != null)
    {
      this.Logger.Error("Cannot attempt a new connection as the previous connection thread is not null!");
      return false;
    }
    if (this.State != RpcState.Disconnected)
    {
      this.Logger.Warning("Cannot attempt a new connection as the previous connection hasn't changed state yet.");
      return false;
    }
    if (this.aborting)
    {
      this.Logger.Error("Cannot attempt a new connection while aborting!");
      return false;
    }
    this.thread = new Thread(new ThreadStart(this.MainLoop))
    {
      Name = "Discord IPC Thread",
      IsBackground = true
    };
    this.thread.Start();
    return true;
  }
  private void SetConnectionState(RpcState state)
  {
    this.Logger.Trace("Setting the connection state to {0}", (object) state.ToString().ToSnakeCase().ToUpperInvariant());
    lock (this.l_states)
      this._state = state;
  }
  public void Shutdown()
  {
    this.Logger.Trace("Initiated shutdown procedure");
    this.shutdown = true;
    lock (this.l_rtqueue)
    {
      this._rtqueue.Clear();
      if (RpcConnection.CLEAR_ON_SHUTDOWN)
        this._rtqueue.Enqueue((ICommand) new PresenceCommand()
        {
          PID = this.processID,
          Presence = (RichPresence) null
        });
      this._rtqueue.Enqueue((ICommand) new CloseCommand());
    }
    this.queueUpdatedEvent.Set();
  }
  public void Close()
  {
    if (this.thread == null)
      this.Logger.Error("Cannot close as it is not available!");
    else if (this.aborting)
      this.Logger.Error("Cannot abort as it has already been aborted");
    else if (this.ShutdownOnly)
    {
      this.Shutdown();
    }
    else
    {
      this.Logger.Trace("Updating Abort State...");
      this.aborting = true;
      this.queueUpdatedEvent.Set();
    }
  }
  public void Dispose()
  {
    this.ShutdownOnly = false;
    this.Close();
  }
}
