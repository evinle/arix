using ArixBack.Models;
using ArixBack.Services;
using ArixBack.Services.Questions;
using Xunit;

namespace ArixBack.Tests;

public class ClassEffectServiceTests
{
    private readonly ClassEffectService _sut = new();

    private static PlayerMatchState MakePlayer(ClassType cls) => new()
    {
        PlayerId = Guid.NewGuid().ToString(),
        ClassType = cls,
        Hp = 100,
    };

    // ── Rogue ─────────────────────────────────────────────────────────────

    [Fact]
    public void Rogue_StreakBelow3_NoBleedApplied()
    {
        // Arrange
        var attacker = MakePlayer(ClassType.Rogue);
        var defender = MakePlayer(ClassType.Wizard);
        attacker.CorrectStreak = 0;

        // Act
        var result = _sut.ApplyOnCorrectAnswer(attacker, defender, 10);

        // Assert
        Assert.Equal(0, defender.BleedStacks);
        Assert.Null(result.EffectMessage);
    }

    [Fact]
    public void Rogue_StreakReaches3_BleedApplied()
    {
        // Arrange
        var attacker = MakePlayer(ClassType.Rogue);
        var defender = MakePlayer(ClassType.Wizard);
        attacker.CorrectStreak = 2; // will become 3 inside Apply

        // Act
        var result = _sut.ApplyOnCorrectAnswer(attacker, defender, 10);

        // Assert
        Assert.Equal(1, defender.BleedStacks);
        Assert.Contains("bleed_applied", result.EffectMessage);
    }

    [Fact]
    public void Rogue_BleedStacks_DoNotExceedMax3()
    {
        // Arrange
        var attacker = MakePlayer(ClassType.Rogue);
        var defender = MakePlayer(ClassType.Wizard);
        defender.BleedStacks = 3; // already at max
        attacker.CorrectStreak = 2;

        // Act
        _sut.ApplyOnCorrectAnswer(attacker, defender, 10);

        // Assert
        Assert.Equal(3, defender.BleedStacks);
    }

    // ── Berserker ─────────────────────────────────────────────────────────

    [Fact]
    public void Berserker_CorrectAnswer_AccumulatesCharge_NoDamage()
    {
        // Arrange
        var attacker = MakePlayer(ClassType.Berserker);
        var defender = MakePlayer(ClassType.Rogue);

        // Act
        var result = _sut.ApplyOnCorrectAnswer(attacker, defender, 10);

        // Assert
        Assert.Equal(0, result.DamageToOpponent);
        Assert.Equal(15, attacker.ChargePoints);
        Assert.Contains("charge:15", result.EffectMessage);
    }

    [Fact]
    public void Berserker_ReleaseCharge_ReturnsDamageAndResetsToZero()
    {
        // Arrange
        var berserker = MakePlayer(ClassType.Berserker);
        berserker.ChargePoints = 45;

        // Act
        int damage = _sut.ReleaseCharge(berserker);

        // Assert
        Assert.Equal(45, damage);
        Assert.Equal(0, berserker.ChargePoints);
    }

    [Fact]
    public void Berserker_ReleaseCharge_WithZeroCharge_ReturnsZero()
    {
        // Arrange
        var berserker = MakePlayer(ClassType.Berserker);
        berserker.ChargePoints = 0;

        // Act
        int damage = _sut.ReleaseCharge(berserker);

        // Assert
        Assert.Equal(0, damage);
    }

    // ── Juggernaut ────────────────────────────────────────────────────────

    [Fact]
    public void Juggernaut_OnHit_Reflects5_ReducesIncomingBy20Percent()
    {
        // Arrange
        var defender = MakePlayer(ClassType.Juggernaut);

        // Act
        var result = _sut.ApplyOnHit(defender, 10);

        // Assert
        Assert.Equal(5, result.DamageToOpponent);   // reflect
        Assert.Equal(8, result.DamageToSelf);        // 10 * 0.8
        Assert.Equal("juggernaut_reflect", result.EffectMessage);
    }

    [Fact]
    public void NonJuggernaut_OnHit_TakesFullDamage_NoReflect()
    {
        // Arrange
        var defender = MakePlayer(ClassType.Rogue);

        // Act
        var result = _sut.ApplyOnHit(defender, 10);

        // Assert
        Assert.Equal(0, result.DamageToOpponent);
        Assert.Equal(10, result.DamageToSelf);
    }

    // ── Wizard ────────────────────────────────────────────────────────────

    [Fact]
    public void Wizard_CorrectAnswer_Heals5()
    {
        // Arrange
        var attacker = MakePlayer(ClassType.Wizard);
        var defender = MakePlayer(ClassType.Rogue);

        // Act
        var result = _sut.ApplyOnCorrectAnswer(attacker, defender, 10);

        // Assert
        Assert.Equal(5, result.HealSelf);
    }

    [Fact]
    public void Wizard_CurseNotApplied_WhenDefenderAlreadyCursed()
    {
        // Arrange
        var attacker = MakePlayer(ClassType.Wizard);
        var defender = MakePlayer(ClassType.Rogue);
        defender.CursedQuestionsRemaining = 3; // already cursed

        // Act — run many times to ensure curse is never re-applied
        for (int i = 0; i < 100; i++)
            _sut.ApplyOnCorrectAnswer(attacker, defender, 10);

        // Assert — still 3 (or less from ticking, but never re-applied via this path)
        Assert.True(defender.CursedQuestionsRemaining <= 3);
    }

    // ── Bleed tick ────────────────────────────────────────────────────────

    [Fact]
    public void TickBleed_WithStacks_DealsDamageAndDecrementsTicksRemaining()
    {
        // Arrange
        var player = MakePlayer(ClassType.Rogue);
        player.BleedStacks = 2;
        player.BleedTicksRemaining = 4;

        // Act
        int damage = _sut.TickBleed(player);

        // Assert
        Assert.Equal(10, damage); // 2 stacks * 5
        Assert.Equal(3, player.BleedTicksRemaining);
    }

    [Fact]
    public void TickBleed_WhenTicksReachZero_ClearsBleedStacks()
    {
        // Arrange
        var player = MakePlayer(ClassType.Rogue);
        player.BleedStacks = 1;
        player.BleedTicksRemaining = 1;

        // Act
        _sut.TickBleed(player);

        // Assert
        Assert.Equal(0, player.BleedStacks);
        Assert.Equal(0, player.BleedTicksRemaining);
    }

    [Fact]
    public void TickBleed_WithNoStacks_ReturnsZero()
    {
        // Arrange
        var player = MakePlayer(ClassType.Rogue);
        player.BleedStacks = 0;

        // Act
        int damage = _sut.TickBleed(player);

        // Assert
        Assert.Equal(0, damage);
    }
}
