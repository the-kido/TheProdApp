using System;
using System.Collections.Generic;
using System.Text.Json;
using Fleck;
using Godot;

partial class ChromeTabDetector 
{

    public static readonly ChromeTabDetector instance;
    static ChromeTabDetector()
    {
        instance = new();
    }
    
    public bool OnYoutube { get; private set; }

    readonly WebSocketServer server;
    readonly List<IWebSocketConnection> connections = new();
    public event Action<Website> EnteredSite;

    public ChromeTabDetector()
    {
        server = new("ws://0.0.0.0:8082");

        server.Start(
            connection => 
            {
                connections.Add(connection);
                connection.OnMessage = OnMessage;
                connection.OnMessage += (msg) =>
                {
                    if (msg == "Reset Cooldown Less")
                    {
                        connections.ForEach(conection => conection.Send("Reset Cooldown Less"));
                    }
                    if (msg == "Reset Cooldown")
                    {
                        connections.ForEach(conection => conection.Send("Reset Cooldown"));
                    }
                };
            }
        );
    }

    private void OnMessage(string msg) 
    {
        // GD.Print(msg);
        if (msg.StartsWith("Site Entered:"))
        {
            Website website = new(msg["Site Entered:".Length..]);
            EnteredSite?.Invoke(website);
        }

        if (msg == "On Youtube") OnYoutube = true;
        if (msg == "Not On Youtube") OnYoutube = false;
        OnYoutube = msg == "On Youtube";
    }
}

public record Website 
{
    public Website(string message)
    {
        // Thanks ai
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(message);
        if (!data.ContainsKey("url") || !data.ContainsKey("url")) throw new JsonException(message);
        
        url = data["url"];
        title = data["title"];
    }
    
    public readonly string url;
    public readonly string title;
}