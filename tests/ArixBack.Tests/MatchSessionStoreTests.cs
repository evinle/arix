using ArixBack.Services;
using Xunit;

namespace ArixBack.Tests;

public class MatchSessionStoreTests
{
    private static MatchSession MakeSession(string p1Id = "p1", string p2Id = "p2")
    {
        return new MatchSession
        {
            Player1 = new PlayerMatchState { PlayerId = p1Id },
            Player2 = new PlayerMatchState { PlayerId = p2Id },
        };
    }

    [Fact]
    public void AddSession_ThenGetByPlayer_ReturnsSameSession()
    {
        // Arrange
        var store = new MatchSessionStore();
        var session = MakeSession();

        // Act
        store.AddSession(session);
        var found = store.GetSessionByPlayer("p1");

        // Assert
        Assert.NotNull(found);
        Assert.Equal(session.SessionId, found.SessionId);
    }

    [Fact]
    public void GetSessionByPlayer_BothPlayersResolveSameSession()
    {
        // Arrange
        var store = new MatchSessionStore();
        var session = MakeSession();
        store.AddSession(session);

        // Act / Assert
        Assert.Equal(session.SessionId, store.GetSessionByPlayer("p1")!.SessionId);
        Assert.Equal(session.SessionId, store.GetSessionByPlayer("p2")!.SessionId);
    }

    [Fact]
    public void RemoveSession_PlayerLookupReturnsNull()
    {
        // Arrange
        var store = new MatchSessionStore();
        var session = MakeSession();
        store.AddSession(session);

        // Act
        store.RemoveSession(session.SessionId);

        // Assert
        Assert.Null(store.GetSessionByPlayer("p1"));
        Assert.Null(store.GetSessionByPlayer("p2"));
    }

    [Fact]
    public void GetSessionByPlayer_UnknownPlayer_ReturnsNull()
    {
        var store = new MatchSessionStore();
        Assert.Null(store.GetSessionByPlayer("nobody"));
    }

    [Fact]
    public void MatchSession_GetPlayer_ReturnsCorrectState()
    {
        var session = MakeSession("alice", "bob");
        Assert.Equal("alice", session.GetPlayer("alice")!.PlayerId);
        Assert.Equal("bob", session.GetPlayer("bob")!.PlayerId);
        Assert.Null(session.GetPlayer("unknown"));
    }

    [Fact]
    public void MatchSession_GetOpponent_ReturnsOtherPlayer()
    {
        var session = MakeSession("alice", "bob");
        Assert.Equal("bob", session.GetOpponent("alice")!.PlayerId);
        Assert.Equal("alice", session.GetOpponent("bob")!.PlayerId);
        Assert.Null(session.GetOpponent("unknown"));
    }
}
