﻿namespace Ibis;

public interface IListener
{
    Task<string> BeginListeningAsync();
    Task ListenAsync(string sessionId, byte[] audioChunk) => Task.CompletedTask;
}